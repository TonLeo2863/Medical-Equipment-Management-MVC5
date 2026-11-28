using System;
using System.Linq;
using System.Web.Mvc;
using MedicalEquipmentManagement.Data;
using MedicalEquipmentManagement.Helpers;
using MedicalEquipmentManagement.Models;
using MedicalEquipmentManagement.ViewModels;

namespace MedicalEquipmentManagement.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>Tổng hợp dữ liệu vận hành cho người đăng nhập và giới hạn phiếu, chi phí theo vai trò.</summary>
        /// <param name="departmentId">Khoa/phòng hiện tại của thiết bị, hoặc null để xem toàn bộ.</param>
        /// <param name="year">Năm dùng cho thống kê chi phí bảo trì.</param>
        /// <param name="days">Ngưỡng cảnh báo gồm 15 hoặc 30 ngày và bao gồm các hạn đã quá.</param>
        public ActionResult Index(int? departmentId, int? year, int? days)
        {
            using (var db = new MedicalEquipmentContext())
            {
                var role = SessionAccount.GetRole(Session, db);
                if (role == null)
                {
                    if (SessionAccount.HasBinding(HttpContext))
                        Session.Clear();
                    return View();
                }
                var userId = (int)Session["UserId"];
                bool showCosts = role == "ADMIN" || role == "MANAGER";
                var selectedYear = year >= 1900 && year <= 9998 ? year.Value : DateTime.Today.Year;
                var selectedDays = days == 15 ? 15 : 30;
                var deadline = DateTime.Today.AddDays(selectedDays);
                var departments = db.Departments.OrderBy(d => d.Name)
                    .Select(d => new { d.DepartmentId, d.Name }).ToList();
                if (departmentId.HasValue && !departments.Any(d => d.DepartmentId == departmentId))
                    return HttpNotFound();

                var assets = db.EquipmentAssets.Where(a => !a.IsDeleted
                    && (!departmentId.HasValue || a.DepartmentId == departmentId));
                var stateCounts = assets.GroupBy(a => a.Status).Select(g => new DashboardCount
                {
                    Label = g.Key, Count = g.Count()
                }).ToList();
                var states = AssetStatus.All.Select(s => stateCounts.FirstOrDefault(c => c.Label == s)
                    ?? new DashboardCount { Label = s }).Concat(stateCounts.Where(c => !AssetStatus.All.Contains(c.Label))).ToList();

                var orders = db.WorkOrders.Where(w => !w.EquipmentAsset.IsDeleted
                    && (!departmentId.HasValue || w.EquipmentAsset.DepartmentId == departmentId)
                    && w.Status != WorkOrderStatus.Completed && w.Status != WorkOrderStatus.Cancelled);
                if (role == "STAFF")
                    orders = orders.Where(w => w.ReporterVerified && w.ReportedByUserId == userId);
                else if (role == "ENGINEER")
                    orders = orders.Where(w => w.AssignedToUserId == userId);

                var eligible = assets.Where(a => a.Status != AssetStatus.Liquidated).Select(a => a.AssetId);
                var maintenance = db.LatestMaintenance().Where(m => eligible.Contains(m.AssetId) && m.NextMaintenanceDate <= deadline);
                var calibrations = db.LatestCalibrations().Where(c => eligible.Contains(c.AssetId) && c.NextCalibrationDate <= deadline);
                var alerts = maintenance.Select(m => new DashboardAlert
                {
                    AssetId = m.AssetId, AssetCode = m.EquipmentAsset.AssetCode, Type = "Bảo trì", Due = m.NextMaintenanceDate.Value
                }).Concat(calibrations.Select(c => new DashboardAlert
                {
                    AssetId = c.AssetId, AssetCode = c.EquipmentAsset.AssetCode, Type = "Hiệu chuẩn", Due = c.NextCalibrationDate.Value
                })).OrderBy(a => a.Due).ThenBy(a => a.AssetCode).Take(30).ToList();

                var monthly = new decimal[12];
                if (showCosts)
                {
                    var start = new DateTime(selectedYear, 1, 1);
                    var end = start.AddYears(1);
                    var costs = db.MaintenanceRecords.Where(m => m.MaintenanceDate >= start && m.MaintenanceDate < end
                        && (!departmentId.HasValue || m.EquipmentAsset.DepartmentId == departmentId))
                        .GroupBy(m => m.MaintenanceDate.Month).Select(g => new { Month = g.Key, Value = g.Sum(m => m.Cost) }).ToList();
                    foreach (var item in costs)
                        monthly[item.Month - 1] = item.Value.GetValueOrDefault();
                }

                return View(new DashboardViewModel
                {
                    DepartmentId = departmentId, DepartmentOptions = new SelectList(departments, "DepartmentId", "Name", departmentId),
                    Days = selectedDays, Year = selectedYear, ShowCosts = showCosts, AnnualCost = monthly.Sum(), MonthlyCosts = monthly,
                    TotalAssets = states.Sum(s => s.Count), States = states, OpenOrders = orders.Count(),
                    DepartmentCount = departmentId.HasValue ? 1 : departments.Count,
                    Departments = assets.GroupBy(a => a.DepartmentId).Select(g => new { Id = g.Key, Count = g.Count() }).ToList()
                        .Select(g => new DashboardCount
                        {
                            Label = departments.FirstOrDefault(d => d.DepartmentId == g.Id)?.Name ?? "Chưa phân bổ", Count = g.Count
                        }).OrderBy(g => g.Label).ToList(),
                    MaintenanceDue = maintenance.Count(), CalibrationDue = calibrations.Count(), Alerts = alerts
                });
            }
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
