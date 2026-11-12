using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using MedicalEquipmentManagement.Data;
using MedicalEquipmentManagement.Models;

namespace MedicalEquipmentManagement.Controllers
{
    public class MaintenanceController : Controller
    {
        private readonly MedicalEquipmentContext db = new MedicalEquipmentContext();

        // GET: Maintenance - Danh sách phiếu bảo trì
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
            ViewBag.UpcomingCount = db.MaintenanceRecords
                .Where(m => m.NextMaintenanceDate.HasValue && m.NextMaintenanceDate.Value <= upcomingLimit)
                .Select(m => m.AssetId)
                .Distinct()
                .Count();

            ViewBag.CurrentResult = resultStatus;
            ViewBag.CurrentType = type;
            ViewBag.Keyword = keyword;

            return View(records);
        }

        // GET: Maintenance/Create
        public ActionResult Create(int? assetId)
        {
            var assetList = db.EquipmentAssets
                .Include(a => a.EquipmentModel)
                .Include(a => a.Department)
                .ToList()
                .Select(a => new
                {
                    AssetId = a.AssetId,
                    DisplayName = $"[{(string.IsNullOrEmpty(a.AssetCode) ? "AST-" + a.AssetId : a.AssetCode)}] {(a.EquipmentModel != null ? a.EquipmentModel.ModelName : "Thiết bị")} (SN: {a.SerialNumber}) - {(a.Department != null ? a.Department.Name : "Chưa phân bổ")}"
                });

            ViewBag.AssetId = new SelectList(assetList, "AssetId", "DisplayName", assetId);

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

        // POST: Maintenance/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "AssetId,MaintenanceType,MaintenanceDate,NextMaintenanceDate,Description,Cost,ResultStatus,TicketId")] MaintenanceRecord record)
        {
            if (ModelState.IsValid)
            {
                db.MaintenanceRecords.Add(record);

                // Cập nhật trạng thái của thiết bị y tế tương ứng
                var asset = db.EquipmentAssets.Find(record.AssetId);
                if (asset != null)
                {
                    if (record.ResultStatus == "Hoàn thành" || record.ResultStatus == "Đạt")
                    {
                        asset.Status = "Đang hoạt động";
                    }
                    else if (record.ResultStatus == "Đang xử lý" || record.ResultStatus == "Chờ linh kiện")
                    {
                        asset.Status = "Đang bảo trì";
                    }
                }

                db.SaveChanges();
                TempData["Success"] = "Đã lưu phiếu bảo trì và cập nhật trạng thái thiết bị thành công.";
                return RedirectToAction("Index");
            }

            var assetList = db.EquipmentAssets
                .Include(a => a.EquipmentModel)
                .Include(a => a.Department)
                .ToList()
                .Select(a => new
                {
                    AssetId = a.AssetId,
                    DisplayName = $"[{(string.IsNullOrEmpty(a.AssetCode) ? "AST-" + a.AssetId : a.AssetCode)}] {(a.EquipmentModel != null ? a.EquipmentModel.ModelName : "Thiết bị")} (SN: {a.SerialNumber}) - {(a.Department != null ? a.Department.Name : "Chưa phân bổ")}"
                });

            ViewBag.AssetId = new SelectList(assetList, "AssetId", "DisplayName", record.AssetId);
            return View(record);
        }

        // GET: Maintenance/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var record = db.MaintenanceRecords
                .Include(m => m.EquipmentAsset.EquipmentModel)
                .Include(m => m.EquipmentAsset.Department)
                .FirstOrDefault(m => m.MaintenanceId == id.Value);

            if (record == null)
            {
                return HttpNotFound();
            }

            return View(record);
        }

        // GET: Maintenance/Upcoming - Lọc thiết bị sắp đến hạn bảo trì (hỗ trợ lọc 15 ngày, 30 ngày)
        public ActionResult Upcoming(int? days)
        {
            var maxDays = days.HasValue && days.Value > 0 ? days.Value : 30;
            var today = DateTime.Today;
            var limitDate = today.AddDays(maxDays);

            var upcomingList = db.MaintenanceRecords
                .Include(m => m.EquipmentAsset.EquipmentModel)
                .Include(m => m.EquipmentAsset.Department)
                .Where(m => m.NextMaintenanceDate.HasValue && m.NextMaintenanceDate.Value <= limitDate)
                .OrderBy(m => m.NextMaintenanceDate)
                .ToList();

            ViewBag.Today = today;
            ViewBag.Days = maxDays;
            return View(upcomingList);
        }

        // GET: Maintenance/DeviceHistory/5 - Xem toàn bộ lịch sử bảo trì & điều chuyển của thiết bị
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
            ViewBag.WorkOrders = db.WorkOrders
                .Include(w => w.AssignedUser)
                .Where(w => w.AssetId == id)
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
