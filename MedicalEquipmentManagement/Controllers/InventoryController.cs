using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using MedicalEquipmentManagement.Data;
using MedicalEquipmentManagement.Models;

namespace MedicalEquipmentManagement.Controllers
{
    public class InventoryController : Controller
    {
        private readonly MedicalEquipmentContext db = new MedicalEquipmentContext();

        // GET: Inventory - Dashboard phân bổ tài sản thiết bị theo Khoa / Phòng ban
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

        // GET: Inventory/Transfer - Điều chuyển thiết bị sang khoa khác
        public ActionResult Transfer(int? assetId, int? fromDepartmentId)
        {
            var availableAssetsQuery = db.EquipmentAssets.Include(a => a.EquipmentModel).Include(a => a.Department).AsQueryable();

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
            ViewBag.FromDepartmentList = new SelectList(db.Departments.Where(d => d.IsActive).OrderBy(d => d.Name), "DepartmentId", "Name", fromDepartmentId);
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

        // POST: Inventory/Transfer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Transfer([Bind(Include = "AssetId,FromDepartmentId,ToDepartmentId,TransferDate,Reason")] AssetTransfer transfer)
        {
            var asset = db.EquipmentAssets.Include(a => a.EquipmentModel).FirstOrDefault(a => a.AssetId == transfer.AssetId);

            if (asset == null)
            {
                ModelState.AddModelError("AssetId", "Thiết bị được chọn không tồn tại.");
            }
            else
            {
                if (asset.Status == "Đang bảo trì")
                {
                    ModelState.AddModelError("", "Thiết bị hiện đang trong quá trình bảo trì/sửa chữa. Không thể điều chuyển lúc này.");
                }

                if (transfer.FromDepartmentId == transfer.ToDepartmentId)
                {
                    ModelState.AddModelError("ToDepartmentId", "Khoa chuyển đến không được trùng với khoa hiện tại.");
                }
            }

            if (ModelState.IsValid)
            {
                // Cập nhật vị trí phòng ban cho thiết bị
                asset.DepartmentId = transfer.ToDepartmentId;
                if (asset.Status == "Chưa phân bổ" || string.IsNullOrEmpty(asset.Status))
                {
                    asset.Status = "Đang hoạt động";
                }

                // Ghi nhận lịch sử luân chuyển
                transfer.TransferDate = DateTime.Now;
                transfer.Status = "Đã chuyển";
                if (!transfer.RequestedBy.HasValue || transfer.RequestedBy.Value == 0)
                {
                    transfer.RequestedBy = 1; // Mặc định là Admin trong hệ thống
                }
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

                var toDept = db.Departments.Find(transfer.ToDepartmentId);
                TempData["Success"] = $"Đã điều chuyển thiết bị '{asset.SerialNumber}' sang {toDept?.Name} thành công.";
                return RedirectToAction("Index", new { departmentId = transfer.ToDepartmentId });
            }

            // Reload dropdown khi có lỗi
            var assetList = db.EquipmentAssets.Include(a => a.EquipmentModel).Include(a => a.Department).ToList().Select(a => new
            {
                AssetId = a.AssetId,
                DisplayName = $"[{(string.IsNullOrEmpty(a.AssetCode) ? "AST-" + a.AssetId : a.AssetCode)}] {(a.EquipmentModel != null ? a.EquipmentModel.ModelName : "Thiết bị")} (SN: {a.SerialNumber}) - {(a.Department != null ? a.Department.Name : "Chưa phân bổ")}"
            });

            ViewBag.AssetId = new SelectList(assetList, "AssetId", "DisplayName", transfer.AssetId);
            ViewBag.FromDepartmentList = new SelectList(db.Departments.Where(d => d.IsActive).OrderBy(d => d.Name), "DepartmentId", "Name", transfer.FromDepartmentId);
            ViewBag.ToDepartmentId = new SelectList(db.Departments.Where(d => d.IsActive).OrderBy(d => d.Name), "DepartmentId", "Name", transfer.ToDepartmentId);

            return View(transfer);
        }

        // GET: Inventory/TransferHistory - Lịch sử điều chuyển
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
