using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using MedicalEquipmentManagement.Data;
using MedicalEquipmentManagement.Filters;
using MedicalEquipmentManagement.Models;

namespace MedicalEquipmentManagement.Controllers
{
    /// <summary>Quản lý kết quả hiệu chuẩn và cảnh báo hạn theo lần hiệu chuẩn mới nhất.</summary>
    [CustomAuthorize(Roles = "ADMIN,MANAGER,ENGINEER")]
    public class CalibrationController : Controller
    {
        private readonly MedicalEquipmentContext db = new MedicalEquipmentContext();

        [HttpGet]
        public ActionResult Index(int? assetId, int? days)
        {
            var records = db.CalibrationLogs.AsQueryable();
            if (assetId.HasValue)
                records = records.Where(c => c.AssetId == assetId);
            if (days == 15 || days == 30)
            {
                var deadline = DateTime.Today.AddDays(days.Value);
                records = db.LatestCalibrations().Where(c => !c.EquipmentAsset.IsDeleted
                    && c.EquipmentAsset.Status != AssetStatus.Liquidated && c.NextCalibrationDate <= deadline
                    && (!assetId.HasValue || c.AssetId == assetId));
            }
            ViewBag.AssetId = assetId;
            return View(records.Include(c => c.EquipmentAsset).OrderByDescending(c => c.CalibrationDate)
                .ThenByDescending(c => c.CalibrationId).ToList());
        }

        [HttpGet, CustomAuthorize(Roles = "ADMIN,ENGINEER")]
        public ActionResult Create(int? assetId)
        {
            LoadAssets(assetId);
            return View(new CalibrationLog { AssetId = assetId.GetValueOrDefault() });
        }

        /// <summary>Lưu lần hiệu chuẩn và đánh dấu thiết bị hỏng nếu kết quả mới nhất không đạt.</summary>
        [HttpPost, ValidateAntiForgeryToken, CustomAuthorize(Roles = "ADMIN,ENGINEER")]
        public ActionResult Create([Bind(Include = "AssetId,CalibrationDate,NextCalibrationDate,PerformedBy,CertificateNumber,Result")] CalibrationLog input)
        {
            if (input.CalibrationDate == DateTime.MinValue || input.CalibrationDate > DateTime.Today)
                ModelState.AddModelError(nameof(input.CalibrationDate), "Ngày hiệu chuẩn phải có và không sau hôm nay.");
            if (!input.NextCalibrationDate.HasValue || input.NextCalibrationDate < input.CalibrationDate)
                ModelState.AddModelError(nameof(input.NextCalibrationDate), "Ngày kế tiếp phải bằng hoặc sau ngày hiệu chuẩn.");
            if (input.Result != "Đạt" && input.Result != "Không đạt")
                ModelState.AddModelError(nameof(input.Result), "Chọn kết quả hiệu chuẩn.");
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
                        input.CalibrationDate = input.CalibrationDate.Date;
                        input.NextCalibrationDate = input.NextCalibrationDate.Value.Date;
                        input.PerformedBy = input.PerformedBy.Trim();
                        bool latest = !db.CalibrationLogs.Any(c => c.AssetId == input.AssetId && c.CalibrationDate > input.CalibrationDate);
                        if (latest && input.Result == "Không đạt")
                            asset.Status = AssetStatus.Broken;
                        db.CalibrationLogs.Add(input);
                        db.SaveChanges();
                        transaction.Commit();
                        TempData["Success"] = "Đã ghi nhận hiệu chuẩn.";
                        return RedirectToAction("Index", new { assetId = input.AssetId });
                    }
                }
            }
            LoadAssets(input.AssetId);
            return View(input);
        }

        private void LoadAssets(int? selected)
        {
            var choices = db.EquipmentAssets.Where(a => !a.IsDeleted && a.Status != AssetStatus.Liquidated)
                .OrderBy(a => a.AssetCode).Select(a => new { a.AssetId, Label = a.AssetCode + " · " + a.EquipmentModel.ModelName }).ToList();
            ViewBag.Assets = new SelectList(choices, "AssetId", "Label", selected);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}