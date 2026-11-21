using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using MedicalEquipmentManagement.Data;
using MedicalEquipmentManagement.Models;

namespace MedicalEquipmentManagement.Controllers
{
    public class WorkOrderController : Controller
    {
        private readonly MedicalEquipmentContext db = new MedicalEquipmentContext();

        // GET: WorkOrder - Danh sách phiếu yêu cầu sửa chữa
        public ActionResult Index(string status, string keyword)
        {
            var query = db.WorkOrders
                .Include(w => w.EquipmentAsset.EquipmentModel)
                .Include(w => w.EquipmentAsset.Department)
                .Include(w => w.AssignedUser)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(w => w.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(w => w.IssueDescription.Contains(keyword) ||
                                         w.EquipmentAsset.SerialNumber.Contains(keyword) ||
                                         (w.EquipmentAsset.AssetCode != null && w.EquipmentAsset.AssetCode.Contains(keyword)));
            }

            var orders = query.OrderByDescending(w => w.CreatedDate).ToList();

            // Thống kê trạng thái phiếu
            ViewBag.TotalOrders = db.WorkOrders.Count();
            ViewBag.PendingOrders = db.WorkOrders.Count(w => w.Status == "Chờ xử lý");
            ViewBag.InRepairOrders = db.WorkOrders.Count(w => w.Status == "Đang sửa");
            ViewBag.CompletedOrders = db.WorkOrders.Count(w => w.Status == "Đã xong");

            ViewBag.CurrentStatus = status;
            ViewBag.Keyword = keyword;

            return View(orders);
        }

        // GET: WorkOrder/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var workOrder = db.WorkOrders
                .Include(w => w.EquipmentAsset.EquipmentModel)
                .Include(w => w.EquipmentAsset.Department)
                .Include(w => w.AssignedUser)
                .FirstOrDefault(w => w.WorkOrderId == id.Value);

            if (workOrder == null)
            {
                return HttpNotFound();
            }

            return View(workOrder);
        }

        // GET: WorkOrder/Create
        public ActionResult Create(int? assetId)
        {
            var assetList = db.EquipmentAssets
                .Include(a => a.EquipmentModel)
                .Include(a => a.Department)
                .ToList()
                .Select(a => new
                {
                    AssetId = a.AssetId,
                    DisplayName = $"[{(string.IsNullOrEmpty(a.AssetCode) ? "AST-" + a.AssetId : a.AssetCode)}] {(a.EquipmentModel != null ? a.EquipmentModel.ModelName : "Thiết bị")} (SN: {a.SerialNumber}) - {(a.Department != null ? a.Department.Name : "Chưa giao khoa")}"
                });

            ViewBag.AssetId = new SelectList(assetList, "AssetId", "DisplayName", assetId);
            ViewBag.AssignedToUserId = new SelectList(db.Users.OrderBy(u => u.FullName), "UserId", "FullName");

            var model = new WorkOrder
            {
                CreatedDate = DateTime.Now,
                Status = "Chờ xử lý"
            };

            if (assetId.HasValue)
            {
                model.AssetId = assetId.Value;
            }

            return View(model);
        }

        // POST: WorkOrder/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "AssetId,IssueDescription,CreatedDate,Status,AssignedToUserId")] WorkOrder workOrder)
        {
            if (ModelState.IsValid)
            {
                db.WorkOrders.Add(workOrder);

                // Cập nhật trạng thái máy nếu phiếu đang báo hỏng/đang sửa
                var asset = db.EquipmentAssets.Find(workOrder.AssetId);
                if (asset != null && (workOrder.Status == "Chờ xử lý" || workOrder.Status == "Đang sửa"))
                {
                    asset.Status = "Đang bảo trì";
                }

                db.SaveChanges();
                TempData["Success"] = "Đã tạo phiếu yêu cầu sửa chữa thiết bị thành công.";
                return RedirectToAction("Index");
            }

            var assetList = db.EquipmentAssets
                .Include(a => a.EquipmentModel)
                .Include(a => a.Department)
                .ToList()
                .Select(a => new
                {
                    AssetId = a.AssetId,
                    DisplayName = $"[{(string.IsNullOrEmpty(a.AssetCode) ? "AST-" + a.AssetId : a.AssetCode)}] {(a.EquipmentModel != null ? a.EquipmentModel.ModelName : "Thiết bị")} (SN: {a.SerialNumber}) - {(a.Department != null ? a.Department.Name : "Chưa giao khoa")}"
                });

            ViewBag.AssetId = new SelectList(assetList, "AssetId", "DisplayName", workOrder.AssetId);
            ViewBag.AssignedToUserId = new SelectList(db.Users.OrderBy(u => u.FullName), "UserId", "FullName", workOrder.AssignedToUserId);
            return View(workOrder);
        }

        // GET: WorkOrder/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var workOrder = db.WorkOrders.Find(id);
            if (workOrder == null)
            {
                return HttpNotFound();
            }

            var assetList = db.EquipmentAssets
                .Include(a => a.EquipmentModel)
                .Include(a => a.Department)
                .ToList()
                .Select(a => new
                {
                    AssetId = a.AssetId,
                    DisplayName = $"[{(string.IsNullOrEmpty(a.AssetCode) ? "AST-" + a.AssetId : a.AssetCode)}] {(a.EquipmentModel != null ? a.EquipmentModel.ModelName : "Thiết bị")} (SN: {a.SerialNumber}) - {(a.Department != null ? a.Department.Name : "Chưa giao khoa")}"
                });

            ViewBag.AssetId = new SelectList(assetList, "AssetId", "DisplayName", workOrder.AssetId);
            ViewBag.AssignedToUserId = new SelectList(db.Users.OrderBy(u => u.FullName), "UserId", "FullName", workOrder.AssignedToUserId);

            return View(workOrder);
        }

        // POST: WorkOrder/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "WorkOrderId,AssetId,IssueDescription,CreatedDate,Status,AssignedToUserId")] WorkOrder workOrder)
        {
            if (ModelState.IsValid)
            {
                db.Entry(workOrder).State = EntityState.Modified;

                // Tự động đồng bộ trạng thái của thiết bị
                var asset = db.EquipmentAssets.Find(workOrder.AssetId);
                if (asset != null)
                {
                    if (workOrder.Status == "Đã xong")
                    {
                        asset.Status = "Đang hoạt động";
                    }
                    else if (workOrder.Status == "Đang sửa" || workOrder.Status == "Chờ xử lý")
                    {
                        asset.Status = "Đang bảo trì";
                    }
                }

                db.SaveChanges();
                TempData["Success"] = "Đã cập nhật tiến độ phiếu yêu cầu sửa chữa thành công.";
                return RedirectToAction("Index");
            }

            var assetList = db.EquipmentAssets
                .Include(a => a.EquipmentModel)
                .Include(a => a.Department)
                .ToList()
                .Select(a => new
                {
                    AssetId = a.AssetId,
                    DisplayName = $"[{(string.IsNullOrEmpty(a.AssetCode) ? "AST-" + a.AssetId : a.AssetCode)}] {(a.EquipmentModel != null ? a.EquipmentModel.ModelName : "Thiết bị")} (SN: {a.SerialNumber}) - {(a.Department != null ? a.Department.Name : "Chưa giao khoa")}"
                });

            ViewBag.AssetId = new SelectList(assetList, "AssetId", "DisplayName", workOrder.AssetId);
            ViewBag.AssignedToUserId = new SelectList(db.Users.OrderBy(u => u.FullName), "UserId", "FullName", workOrder.AssignedToUserId);

            return View(workOrder);
        }

        // GET: WorkOrder/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var workOrder = db.WorkOrders
                .Include(w => w.EquipmentAsset.EquipmentModel)
                .FirstOrDefault(w => w.WorkOrderId == id.Value);

            if (workOrder == null)
            {
                return HttpNotFound();
            }

            return View(workOrder);
        }

        // POST: WorkOrder/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var workOrder = db.WorkOrders.Find(id);
            if (workOrder != null)
            {
                db.WorkOrders.Remove(workOrder);
                db.SaveChanges();
                TempData["Success"] = "Đã xóa phiếu yêu cầu sửa chữa.";
            }

            return RedirectToAction("Index");
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
