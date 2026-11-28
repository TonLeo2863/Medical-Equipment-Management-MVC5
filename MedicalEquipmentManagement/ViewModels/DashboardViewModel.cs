using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace MedicalEquipmentManagement.ViewModels
{
    /// <summary>Chứa số liệu tổng hợp và bộ lọc của Dashboard, giới hạn theo quyền từ Controller.</summary>
    public class DashboardViewModel
    {
        public int TotalAssets { get; set; }
        public int OpenOrders { get; set; }
        public int DepartmentCount { get; set; }
        public int MaintenanceDue { get; set; }
        public int CalibrationDue { get; set; }
        public int? DepartmentId { get; set; }
        public int Year { get; set; }
        public int Days { get; set; }
        public bool ShowCosts { get; set; }
        public decimal AnnualCost { get; set; }
        public IEnumerable<SelectListItem> DepartmentOptions { get; set; }
        public IList<DashboardCount> States { get; set; }
        public IList<DashboardCount> Departments { get; set; }
        public decimal[] MonthlyCosts { get; set; }
        public IList<DashboardAlert> Alerts { get; set; }
    }

    public class DashboardCount
    {
        public string Label { get; set; }
        public int Count { get; set; }
    }

    public class DashboardAlert
    {
        public int AssetId { get; set; }
        public string AssetCode { get; set; }
        public string Type { get; set; }
        public DateTime Due { get; set; }
    }
}