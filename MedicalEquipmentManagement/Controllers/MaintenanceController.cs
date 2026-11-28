using MedicalEquipmentManagement.Filters;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using MedicalEquipmentManagement.Data;
using MedicalEquipmentManagement.Models;
using MedicalEquipmentManagement.Helpers;

namespace MedicalEquipmentManagement.Controllers
{
    /// <summary>Ghi nhận lịch sử bảo trì, chi phí, người thực hiện và lịch tiếp theo của từng thiết bị.</summary>
    [CustomAuthorize(Roles = "ADMIN,MANAGER,ENGINEER")]
    public class MaintenanceController : Controller
    {
        private readonly MedicalEquipmentContext db = new MedicalEquipmentContext();

        public ActionResult Index(string resultStatus, string type, string keyword)
        {
            var query = db.MaintenanceRecords
                .Include(m => m.EquipmentAsset.EquipmentModel)
                .Include(m => m.EquipmentAsset.Department)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(resultStatus))
            {
                query = query.Where(m => m.ResultStatus == resultStatus);
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(m => m.MaintenanceType == type);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(m => m.Description.Contains(keyword) ||
                                         m.EquipmentAsset.SerialNumber.Contains(keyword) ||
                                         (m.EquipmentAsset.AssetCode != null && m.EquipmentAsset.AssetCode.Contains(keyword)));
            }

            var records = query.OrderByDescending(m => m.MaintenanceDate).ToList();

            // Thống kê bằng LINQ
            ViewBag.TotalRecords = db.MaintenanceRecords.Count();
            ViewBag.CompletedRecords = db.MaintenanceRecords.Count(m => m.ResultStatus == "Hoàn thành" || m.ResultStatus == "Đạt");
            ViewBag.InProgressRecords = db.MaintenanceRecords.Count(m => m.ResultStatus != "Hoàn thành" && m.ResultStatus != "Đạt");
            ViewBag.TotalCost = db.MaintenanceRecords.Sum(m => m.Cost) ?? 0;

            // Đếm thiết bị sắp đến hạn bảo trì trong vòng 30 ngày
            var today = DateTime.Today;
            var upcomingLimit = today.AddDays(30);
            ViewBag.UpcomingCount = DueMaintenance(upcomingLimit)
                .Count();

            ViewBag.CurrentResult = resultStatus;
            ViewBag.CurrentType = type;
            ViewBag.Keyword = keyword;

            return View(records);
        }

        [HttpGet, CustomAuthorize(Roles = "ADMIN,ENGINEER")]
        public ActionResult Create(int? assetId)
        {
            var assetList = db.EquipmentAssets
                .Where(a => !a.IsDeleted && a.Status != AssetStatus.Liquidated)
                .Include(a => a.EquipmentModel)
                .Include(a => a.Department)
                .ToList()
                .Select(a => new
                {
                    AssetId = a.AssetId,
                    DisplayName = $"[{(string.IsNullOrEmpty(a.AssetCode) ? "AST-" + a.AssetId : a.AssetCode)}] {(a.EquipmentModel != null ? a.EquipmentModel.ModelName : "Thiết bị")} (SN: {a.SerialNumber}) - {(a.Department != null ? a.Department.Name : "Chưa phân bổ")}"
                });

            ViewBag.AssetId = new SelectList(assetList, "AssetId", "DisplayName", assetId);
            LoadTickets(null);

            var model = new MaintenanceRecord
            {
                MaintenanceDate = DateTime.Today,
                NextMaintenanceDate = DateTime.Today.AddMonths(6),
                MaintenanceType = "Bảo dưỡng định kỳ",
                ResultStatus = "Hoàn thành"
            };

            if (assetId.HasValue)
            {
                model.AssetId = assetId.Value;
            }

            return View(model);
        }

        /// <summary>Lưu bảo trì và đánh giá lại trạng thái tài sản trong cùng transaction. Phiếu liên kết phải thuộc đúng thiết bị.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "ADMIN,ENGINEER")]
        public ActionResult Create([Bind(Include = "AssetId,MaintenanceType,MaintenanceDate,NextMaintenanceDate,Description,Cost,ResultStatus,TicketId")] MaintenanceRecord record)
        {
            if (record.MaintenanceDate == DateTime.MinValue || record.MaintenanceDate > DateTime.Today)
                ModelState.AddModelError(nameof(record.MaintenanceDate), "Ngày thực hiện phải có và không sau hôm nay.");
            if (record.NextMaintenanceDate < record.MaintenanceDate)
                ModelState.AddModelError(nameof(record.NextMaintenanceDate), "Ngày kế tiếp không được trước ngày thực hiện.");
            if (!new[] { "Hoàn thành", "Đang xử lý", "Chờ linh kiện", "Không thể khắc phục" }.Contains(record.ResultStatus))
                ModelState.AddModelError(nameof(record.ResultStatus), "Vui lòng chọn kết quả hợp lệ.");
            if (ModelState.IsValid)
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    db.LockAsset(record.AssetId);
                    var asset = db.EquipmentAssets.Find(record.AssetId);
                    var userId = (int)Session["UserId"];
                    var isAdmin = (string)Session["RoleCode"] == "ADMIN";
                    if (asset == null || asset.IsDeleted || asset.Status == AssetStatus.Liquidated)
                        ModelState.AddModelError(nameof(record.AssetId), "Thiết bị không còn khả dụng.");
                    else if (record.TicketId.HasValue && !db.WorkOrders.Any(w => w.WorkOrderId == record.TicketId
                        && w.AssetId == record.AssetId && (w.AssignedToUserId == userId || isAdmin)))
                        ModelState.AddModelError(nameof(record.TicketId), "Phiếu không thuộc thiết bị hoặc nằm ngoài phạm vi được phép.");
                    else
                    {
                        record.MaintenanceDate = record.MaintenanceDate.Date;
                        record.NextMaintenanceDate = record.NextMaintenanceDate?.Date;
                        record.PerformedByUserId = userId;
                        if (!db.MaintenanceRecords.Any(m => m.AssetId == record.AssetId && m.MaintenanceDate > record.MaintenanceDate))
                        {
                            if (record.ResultStatus == "Không thể khắc phục")
                                asset.Status = AssetStatus.Broken;
                            else if (record.ResultStatus != "Hoàn thành")
                                asset.Status = AssetStatus.Maintenance;
                            else if (!EquipmentPolicy.HasOpenOrder(db, asset.AssetId)
                                && !EquipmentPolicy.HasFailedCalibration(db, asset.AssetId) && !EquipmentPolicy.HasFailedRepair(db, asset.AssetId))
                                asset.Status = AssetStatus.Active;
                        }
                        db.MaintenanceRecords.Add(record);
                        db.SaveChanges();
                        transaction.Commit();
                        TempData["Success"] = "Đã ghi nhận bảo trì.";
                        return RedirectToAction("Details", new { id = record.MaintenanceId });
                    }
                }
            }

            var assetList = db.EquipmentAssets
                .Where(a => !a.IsDeleted && a.Status != AssetStatus.Liquidated)
                .Include(a => a.EquipmentModel)
                .Include(a => a.Department)
                .ToList()
                .Select(a => new
                {
                    AssetId = a.AssetId,
                    DisplayName = $"[{(string.IsNullOrEmpty(a.AssetCode) ? "AST-" + a.AssetId : a.AssetCode)}] {(a.EquipmentModel != null ? a.EquipmentModel.ModelName : "Thiết bị")} (SN: {a.SerialNumber}) - {(a.Department != null ? a.Department.Name : "Chưa phân bổ")}"
                });

            ViewBag.AssetId = new SelectList(assetList, "AssetId", "DisplayName", record.AssetId);
            LoadTickets(record.TicketId);
            return View(record);
        }

        private void LoadTickets(int? selected)
        {
            var userId = (int)Session["UserId"];
            var isAdmin = (string)Session["RoleCode"] == "ADMIN";
            var choices = db.WorkOrders.Where(w => isAdmin || w.AssignedToUserId == userId)
                .OrderByDescending(w => w.CreatedDate)
                .Select(w => new { w.WorkOrderId, Label = w.EquipmentAsset.AssetCode + " · " + w.TicketCode }).ToList();
            ViewBag.Tickets = new SelectList(choices, "WorkOrderId", "Label", selected);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var record = db.MaintenanceRecords
                .Include(m => m.EquipmentAsset.EquipmentModel)
                .Include(m => m.EquipmentAsset.Department)
                .Include(m => m.PerformedBy)
                .Include(m => m.WorkOrder)
                .FirstOrDefault(m => m.MaintenanceId == id.Value);

            if (record == null)
            {
                return HttpNotFound();
            }

            return View(record);
        }

        public ActionResult Upcoming(int? days)
        {
            var maxDays = days == 15 ? 15 : 30;
            var today = DateTime.Today;
            var limitDate = today.AddDays(maxDays);

            var upcomingList = DueMaintenance(limitDate)
                .Include(m => m.EquipmentAsset.EquipmentModel)
                .Include(m => m.EquipmentAsset.Department)
                .OrderBy(m => m.NextMaintenanceDate)
                .ToList();

            ViewBag.Today = today;
            ViewBag.Days = maxDays;
            return View(upcomingList);
        }

        private IQueryable<MaintenanceRecord> DueMaintenance(DateTime deadline)
        {
            return db.LatestMaintenance().Where(m => m.NextMaintenanceDate.HasValue && m.NextMaintenanceDate <= deadline
                && !m.EquipmentAsset.IsDeleted && m.EquipmentAsset.Status != AssetStatus.Liquidated);
        }

        public ActionResult DeviceHistory(int id)
        {
            var asset = db.EquipmentAssets
                .Include(a => a.EquipmentModel)
                .Include(a => a.Department)
                .FirstOrDefault(a => a.AssetId == id);

            if (asset == null)
            {
                return HttpNotFound();
            }

            ViewBag.Asset = asset;

            // 1. Lịch sử bảo trì
            var maintenanceHistory = db.MaintenanceRecords
                .Where(m => m.AssetId == id)
                .OrderByDescending(m => m.MaintenanceDate)
                .ToList();

            // 2. Lịch sử luân chuyển phòng ban
            ViewBag.TransferHistory = db.AssetTransfers
                .Include(t => t.FromDepartment)
                .Include(t => t.ToDepartment)
                .Where(t => t.AssetId == id)
                .OrderByDescending(t => t.TransferDate)
                .ToList();

            // 3. Lịch sử phiếu báo hỏng
            var orders = db.WorkOrders
                .Include(w => w.AssignedUser)
                .Where(w => w.AssetId == id);
            if ((string)Session["RoleCode"] == "ENGINEER")
            {
                var engineerId = (int)Session["UserId"];
                orders = orders.Where(w => w.AssignedToUserId == engineerId);
            }
            ViewBag.WorkOrders = orders
                .OrderByDescending(w => w.CreatedDate)
                .ToList();

            return View(maintenanceHistory);
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
