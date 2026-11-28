using MedicalEquipmentManagement.Filters;
using MedicalEquipmentManagement.Helpers;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using MedicalEquipmentManagement.Data;
using MedicalEquipmentManagement.Models;

namespace MedicalEquipmentManagement.Controllers
{
    /// <summary>Tra cứu vị trí từng tài sản và điều chuyển có nhật ký khoa/phòng và giao dịch kho.</summary>
    [CustomAuthorize(Roles = "ADMIN,MANAGER")]
    public class InventoryController : Controller
    {
        private readonly MedicalEquipmentContext db = new MedicalEquipmentContext();

        public ActionResult Index(int? departmentId, string status, string keyword)
        {
            var query = db.EquipmentAssets
                .Include(a => a.EquipmentModel)
                .Include(a => a.Department)
                .AsQueryable();

            if (departmentId.HasValue)
            {
                query = query.Where(a => a.DepartmentId == departmentId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(a => a.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(a => a.SerialNumber.Contains(keyword) || 
                                         (a.AssetCode != null && a.AssetCode.Contains(keyword)) ||
                                         (a.EquipmentModel != null && a.EquipmentModel.ModelName.Contains(keyword)));
            }

            var assets = query.OrderBy(a => a.AssetId).ToList();

            // Thống kê bằng LINQ
            ViewBag.TotalAssets = db.EquipmentAssets.Count();
            ViewBag.ActiveAssets = db.EquipmentAssets.Count(a => a.Status == "Đang hoạt động");
            ViewBag.MaintenanceAssets = db.EquipmentAssets.Count(a => a.Status == "Đang bảo trì");
            ViewBag.UnassignedAssets = db.EquipmentAssets.Count(a => a.DepartmentId == null);

            ViewBag.DepartmentList = new SelectList(db.Departments.Where(d => d.IsActive).OrderBy(d => d.Name), "DepartmentId", "Name", departmentId);
            ViewBag.CurrentDeptId = departmentId;
            ViewBag.CurrentStatus = status;
            ViewBag.Keyword = keyword;

            return View(assets);
        }

        public ActionResult Transfer(int? assetId, int? fromDepartmentId)
        {
            var availableAssetsQuery = db.EquipmentAssets.Include(a => a.EquipmentModel).Include(a => a.Department)
                .Where(a => !a.IsDeleted && a.Status == AssetStatus.Active && a.DepartmentId.HasValue);

            if (fromDepartmentId.HasValue)
            {
                availableAssetsQuery = availableAssetsQuery.Where(a => a.DepartmentId == fromDepartmentId.Value);
            }

            var assetList = availableAssetsQuery.ToList().Select(a => new
            {
                AssetId = a.AssetId,
                DisplayName = $"[{(string.IsNullOrEmpty(a.AssetCode) ? "AST-" + a.AssetId : a.AssetCode)}] {(a.EquipmentModel != null ? a.EquipmentModel.ModelName : "Thiết bị")} (SN: {a.SerialNumber}) - {(a.Department != null ? a.Department.Name : "Chưa phân bổ")}"
            });

            ViewBag.AssetId = new SelectList(assetList, "AssetId", "DisplayName", assetId);
            ViewBag.FromDepartmentList = new SelectList(db.Departments.OrderBy(d => d.Name), "DepartmentId", "Name", fromDepartmentId);
            ViewBag.ToDepartmentId = new SelectList(db.Departments.Where(d => d.IsActive).OrderBy(d => d.Name), "DepartmentId", "Name");

            var model = new AssetTransfer();
            if (assetId.HasValue)
            {
                var asset = db.EquipmentAssets.Find(assetId.Value);
                if (asset != null)
                {
                    model.AssetId = asset.AssetId;
                    if (asset.DepartmentId.HasValue)
                    {
                        model.FromDepartmentId = asset.DepartmentId.Value;
                    }
                }
            }

            return View(model);
        }

        /// <summary>Khóa dòng tài sản trước khi kiểm tra khoa nguồn, sau đó ghi vị trí, phiếu và nhật ký kho trong một transaction. Dispose rollback nếu chưa Commit.</summary>
        /// <param name="transfer">Tài sản, khoa nguồn, khoa đích và lý do. Người thực hiện lấy từ Session.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Transfer([Bind(Include = "AssetId,FromDepartmentId,ToDepartmentId,TransferDate,Reason")] AssetTransfer transfer)
        {
            if (!(Session["UserId"] is int userId))
                return new HttpUnauthorizedResult();

            using (var transaction = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                db.LockAsset(transfer.AssetId);
                var asset = db.EquipmentAssets.Include(a => a.EquipmentModel).FirstOrDefault(a => a.AssetId == transfer.AssetId && !a.IsDeleted);

                if (asset == null)
                {
                    ModelState.AddModelError("AssetId", "Thiết bị được chọn không tồn tại.");
                }
                else
                {
                    if (asset.Status != AssetStatus.Active || EquipmentPolicy.HasOpenOrder(db, asset.AssetId))
                    {
                        ModelState.AddModelError("", "Chỉ có thể điều chuyển thiết bị đang hoạt động.");
                    }

                    if (asset.DepartmentId != transfer.FromDepartmentId)
                        ModelState.AddModelError("FromDepartmentId", "Khoa nguồn không khớp với vị trí hiện tại.");

                    if (asset.DepartmentId == transfer.ToDepartmentId)
                    {
                        ModelState.AddModelError("ToDepartmentId", "Khoa chuyển đến không được trùng với khoa hiện tại.");
                    }
                }

                if (!db.Departments.Any(d => d.DepartmentId == transfer.ToDepartmentId && d.IsActive))
                    ModelState.AddModelError("ToDepartmentId", "Khoa đích không hoạt động hoặc không tồn tại.");

                if (ModelState.IsValid)
                {
                    // Cập nhật vị trí phòng ban cho thiết bị
                    asset.DepartmentId = transfer.ToDepartmentId;

                    // Ghi nhận lịch sử luân chuyển
                    transfer.TransferDate = DateTime.Now;
                    transfer.Status = "Đã chuyển";
                    transfer.RequestedBy = userId;
                    db.AssetTransfers.Add(transfer);

                    // Đồng bộ ghi log giao dịch kho
                    var invTx = new InventoryTransaction
                    {
                        EquipmentId = asset.AssetId,
                        DepartmentId = transfer.ToDepartmentId,
                        TransactionType = "Điều chuyển",
                        Quantity = 1,
                        TransactionDate = DateTime.Now,
                        Note = $"Chuyển từ khoa ID {transfer.FromDepartmentId} sang khoa ID {transfer.ToDepartmentId}. Lý do: {transfer.Reason}"
                    };
                    db.InventoryTransactions.Add(invTx);

                    db.SaveChanges();
                    transaction.Commit();

                    var toDept = db.Departments.Find(transfer.ToDepartmentId);
                    TempData["Success"] = $"Đã điều chuyển thiết bị '{asset.SerialNumber}' sang {toDept?.Name} thành công.";
                    return RedirectToAction("Index", new { departmentId = transfer.ToDepartmentId });
                }

            }

            // Reload dropdown khi có lỗi
            var assetList = db.EquipmentAssets.Include(a => a.EquipmentModel).Include(a => a.Department)
                .Where(a => !a.IsDeleted && a.Status == AssetStatus.Active && a.DepartmentId.HasValue).ToList().Select(a => new
            {
                AssetId = a.AssetId,
                DisplayName = $"[{(string.IsNullOrEmpty(a.AssetCode) ? "AST-" + a.AssetId : a.AssetCode)}] {(a.EquipmentModel != null ? a.EquipmentModel.ModelName : "Thiết bị")} (SN: {a.SerialNumber}) - {(a.Department != null ? a.Department.Name : "Chưa phân bổ")}"
            });

            ViewBag.AssetId = new SelectList(assetList, "AssetId", "DisplayName", transfer.AssetId);
            ViewBag.FromDepartmentList = new SelectList(db.Departments.OrderBy(d => d.Name), "DepartmentId", "Name", transfer.FromDepartmentId);
            ViewBag.ToDepartmentId = new SelectList(db.Departments.Where(d => d.IsActive).OrderBy(d => d.Name), "DepartmentId", "Name", transfer.ToDepartmentId);

            return View(transfer);
        }

        public ActionResult TransferHistory(int? assetId, int? departmentId)
        {
            var query = db.AssetTransfers
                .Include(t => t.EquipmentAsset.EquipmentModel)
                .Include(t => t.FromDepartment)
                .Include(t => t.ToDepartment)
                .AsQueryable();

            if (assetId.HasValue)
            {
                query = query.Where(t => t.AssetId == assetId.Value);
            }

            if (departmentId.HasValue)
            {
                query = query.Where(t => t.FromDepartmentId == departmentId.Value || t.ToDepartmentId == departmentId.Value);
            }

            var history = query.OrderByDescending(t => t.TransferDate).ToList();

            ViewBag.DepartmentList = new SelectList(db.Departments.OrderBy(d => d.Name), "DepartmentId", "Name", departmentId);
            ViewBag.AssetId = assetId;

            return View(history);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
