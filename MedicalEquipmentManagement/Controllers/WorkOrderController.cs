using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using MedicalEquipmentManagement.Data;
using MedicalEquipmentManagement.Filters;
using MedicalEquipmentManagement.Helpers;
using MedicalEquipmentManagement.Models;

namespace MedicalEquipmentManagement.Controllers
{
    /// <summary>Quản lý phiếu báo hỏng, phân công và cập nhật kết quả sửa chữa theo trạng thái.</summary>
    [CustomAuthorize(Roles = "ADMIN,MANAGER,ENGINEER,STAFF")]
    public class WorkOrderController : Controller
    {
        private readonly MedicalEquipmentContext db = new MedicalEquipmentContext();
        private int UserId => (int)Session["UserId"];
        private bool CanAssign => Session["RoleCode"] as string == "ADMIN" || Session["RoleCode"] as string == "MANAGER";

        private bool CanPerform(WorkOrder order)
        {
            return order.AssignedToUserId.HasValue
                && (order.AssignedToUserId == UserId || (string)Session["RoleCode"] == "ADMIN");
        }

        /// <summary>Giới hạn truy vấn tại SQL Server để STAFF chỉ thấy phiếu của mình và ENGINEER chỉ thấy phiếu được giao.</summary>
        private IQueryable<WorkOrder> VisibleOrders()
        {
            var orders = db.WorkOrders.AsQueryable();
            if (Session["RoleCode"] as string == "STAFF")
            {
                var userId = UserId;
                return orders.Where(w => w.ReporterVerified && w.ReportedByUserId == userId);
            }
            if (Session["RoleCode"] as string == "ENGINEER")
            {
                var userId = UserId;
                return orders.Where(w => w.AssignedToUserId == userId);
            }
            return orders;
        }

        public ActionResult Index(string status, string keyword)
        {
            var orders = VisibleOrders().Include(w => w.EquipmentAsset.EquipmentModel)
                .Include(w => w.EquipmentAsset.Department).Include(w => w.AssignedUser);
            if (!string.IsNullOrWhiteSpace(status))
                orders = orders.Where(w => w.Status == status);
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                orders = orders.Where(w => w.TicketCode.Contains(keyword)
                    || w.EquipmentAsset.SerialNumber.Contains(keyword) || w.IssueDescription.Contains(keyword));
            }
            ViewBag.Status = status;
            ViewBag.Keyword = keyword;
            return View(orders.OrderByDescending(w => w.CreatedDate).ThenByDescending(w => w.WorkOrderId).ToList());
        }

        public ActionResult Details(int id)
        {
            var order = VisibleOrders().Include(w => w.EquipmentAsset.EquipmentModel).Include(w => w.Reporter)
                .Include(w => w.AssignedUser).Include(w => w.ResolvedBy).SingleOrDefault(w => w.WorkOrderId == id);
            if (order == null)
                return HttpNotFound();
            ViewBag.CanEdit = (CanAssign && order.Status == WorkOrderStatus.Pending) || CanPerform(order);
            return View(order);
        }

        [HttpGet]
        [CustomAuthorize(Roles = "ADMIN,MANAGER,STAFF")]
        public ActionResult Create(int? assetId)
        {
            LoadAssets(assetId);
            return View(new WorkOrder { AssetId = assetId.GetValueOrDefault() });
        }

        /// <summary>Tạo phiếu chờ xử lý bằng người báo trong Session và cập nhật trạng thái thiết bị trong cùng transaction.</summary>
        /// <param name="input">Chỉ nhận khóa thiết bị và mô tả sự cố từ biểu mẫu.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "ADMIN,MANAGER,STAFF")]
        public ActionResult Create([Bind(Include = "AssetId,IssueDescription")] WorkOrder input)
        {
            input.Status = WorkOrderStatus.Pending;
            ModelState.Remove(nameof(WorkOrder.Status));
            if (string.IsNullOrWhiteSpace(input.IssueDescription))
                ModelState.AddModelError(nameof(input.IssueDescription), "Vui lòng mô tả sự cố.");
            if (ModelState.IsValid)
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    db.LockAsset(input.AssetId);
                    var asset = db.EquipmentAssets.Find(input.AssetId);
                    if (asset == null || asset.IsDeleted || asset.Status == AssetStatus.Liquidated)
                        ModelState.AddModelError(nameof(input.AssetId), "Thiết bị không còn khả dụng.");
                    else
                    {
                        var order = new WorkOrder
                        {
                            AssetId = asset.AssetId,
                            IssueDescription = input.IssueDescription.Trim(),
                            ReportedByUserId = UserId,
                            ReporterVerified = true,
                            CreatedDate = DateTime.Now,
                            TicketCode = "WO-" + Guid.NewGuid().ToString("N"),
                            UrgencyLevel = "Bình thường",
                            Status = WorkOrderStatus.Pending
                        };
                        db.WorkOrders.Add(order);
                        if (asset.Status != AssetStatus.Maintenance)
                            asset.Status = AssetStatus.Broken;
                        db.SaveChanges();
                        transaction.Commit();
                        TempData["Success"] = "Đã gửi phiếu báo hỏng.";
                        return RedirectToAction("Details", new { id = order.WorkOrderId });
                    }
                }
            }
            LoadAssets(input.AssetId);
            return View(input);
        }

        [HttpGet]
        [CustomAuthorize(Roles = "ADMIN,MANAGER,ENGINEER")]
        public ActionResult Edit(int id)
        {
            var order = VisibleOrders().Include(w => w.EquipmentAsset).SingleOrDefault(w => w.WorkOrderId == id);
            if (order == null)
                return HttpNotFound();
            LoadEdit(order);
            return View(order);
        }

        /// <summary>Phân công, bắt đầu hoặc hoàn thành theo trạng thái hiện tại. Khóa tài sản trước khi kiểm tra các phiếu khác.</summary>
        /// <param name="id">Mã định danh của phiếu cần xử lý.</param>
        /// <param name="command">Lệnh assign, start hoặc complete từ biểu mẫu.</param>
        /// <param name="engineerId">Kỹ sư hoạt động được chọn khi phân công.</param>
        /// <param name="resolution">Kết quả bắt buộc khi hoàn tất.</param>
        /// <param name="technicalPassed">Kết luận kỹ thuật. Thiết bị còn phiếu mở hoặc hiệu chuẩn không đạt sẽ chưa hoạt động lại.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "ADMIN,MANAGER,ENGINEER")]
        public ActionResult Edit(int id, string command, int? engineerId, string resolution, bool? technicalPassed)
        {
            var assetId = VisibleOrders().Where(w => w.WorkOrderId == id).Select(w => (int?)w.AssetId).SingleOrDefault();
            if (!assetId.HasValue)
                return HttpNotFound();
            using (var transaction = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                db.LockAsset(assetId.Value);
                var order = VisibleOrders().Include(w => w.EquipmentAsset).SingleOrDefault(w => w.WorkOrderId == id);
                if (order == null)
                    return HttpNotFound();
                var asset = order.EquipmentAsset;
                if (asset.IsDeleted || asset.Status == AssetStatus.Liquidated || order.Status == WorkOrderStatus.Completed
                    || order.Status == WorkOrderStatus.Cancelled)
                    return new HttpStatusCodeResult(HttpStatusCode.Conflict);
                if (command == "assign" && CanAssign && order.Status == WorkOrderStatus.Pending)
                {
                    if (!engineerId.HasValue || !db.Users.Any(u => u.UserId == engineerId && u.IsActive && u.Role.RoleCode == "ENGINEER"))
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "An active engineer is required.");
                    order.AssignedToUserId = engineerId;
                }
                else if (command == "start" && order.Status == WorkOrderStatus.Pending && CanPerform(order))
                {
                    order.Status = WorkOrderStatus.InProgress;
                    asset.Status = AssetStatus.Maintenance;
                }
                else if (command == "complete" && order.Status == WorkOrderStatus.InProgress && CanPerform(order))
                {
                    if (string.IsNullOrWhiteSpace(resolution) || resolution.Length > 500 || !technicalPassed.HasValue)
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Enter a resolution and choose a technical result.");
                    order.Status = WorkOrderStatus.Completed;
                    order.ResolvedDate = DateTime.Now;
                    order.ResolvedByUserId = UserId;
                    order.Resolution = resolution.Trim();
                    order.TechnicalPassed = technicalPassed;
                    bool otherOpen = db.WorkOrders.Any(w => w.AssetId == asset.AssetId && w.WorkOrderId != id
                        && w.Status != WorkOrderStatus.Completed && w.Status != WorkOrderStatus.Cancelled);
                    if (!technicalPassed.Value || EquipmentPolicy.HasFailedCalibration(db, asset.AssetId)
                        || EquipmentPolicy.HasFailedMaintenance(db, asset.AssetId))
                        asset.Status = AssetStatus.Broken;
                    else
                        asset.Status = otherOpen ? AssetStatus.Maintenance : AssetStatus.Active;
                }
                else
                    return new HttpStatusCodeResult(HttpStatusCode.Conflict);
                db.SaveChanges();
                transaction.Commit();
            }
            return RedirectToAction("Details", new { id });
        }

        /// <summary>Hủy phiếu chưa phân công để giữ nguyên dấu vết báo hỏng và người ra quyết định.</summary>
        [HttpPost, ValidateAntiForgeryToken, CustomAuthorize(Roles = "ADMIN")]
        public ActionResult Cancel(int id, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason) || reason.Length > 500)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var assetId = db.WorkOrders.Where(w => w.WorkOrderId == id).Select(w => (int?)w.AssetId).SingleOrDefault();
            if (!assetId.HasValue)
                return HttpNotFound();

            using (var transaction = db.Database.BeginTransaction())
            {
                db.LockAsset(assetId.Value);
                var order = db.WorkOrders.Find(id);
                if (order.Status != WorkOrderStatus.Pending || order.AssignedToUserId.HasValue)
                    return new HttpStatusCodeResult(HttpStatusCode.Conflict);
                order.Status = WorkOrderStatus.Cancelled;
                order.Resolution = reason.Trim();
                order.ResolvedDate = DateTime.Now;
                order.ResolvedByUserId = UserId;
                db.SaveChanges();
                transaction.Commit();
            }
            return RedirectToAction("Details", new { id });
        }

        private void LoadAssets(int? selected)
        {
            var assets = db.EquipmentAssets.Where(a => !a.IsDeleted && a.Status != AssetStatus.Liquidated)
                .OrderBy(a => a.AssetCode).Select(a => new { a.AssetId, Name = a.AssetCode + " · " + a.EquipmentModel.ModelName }).ToList();
            ViewBag.AssetId = new SelectList(assets, "AssetId", "Name", selected);
        }

        private void LoadEdit(WorkOrder order)
        {
            ViewBag.CanAssign = CanAssign && order.Status == WorkOrderStatus.Pending;
            ViewBag.IsAssigned = CanPerform(order);
            if (ViewBag.CanAssign)
                ViewBag.Engineers = new SelectList(db.Users.Where(u => u.IsActive && u.Role.RoleCode == "ENGINEER")
                    .OrderBy(u => u.FullName).ToList(), "UserId", "FullName", order.AssignedToUserId);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
