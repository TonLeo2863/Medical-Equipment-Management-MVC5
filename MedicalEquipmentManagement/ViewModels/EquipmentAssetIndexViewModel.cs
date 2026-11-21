using System;
using System.Collections.Generic;
using System.Web.Mvc;
using MedicalEquipmentManagement.Models;

namespace MedicalEquipmentManagement.ViewModels
{
    /// <summary>
    /// ViewModel cho trang Danh sách Tài sản thiết bị (Equipment/Index):
    /// Đóng gói Tìm kiếm (theo Tên/Serial), Lọc (theo Khoa/Trạng thái) và Phân trang (Pagination).
    /// </summary>
    public class EquipmentAssetIndexViewModel
    {
        // Tham số Tìm kiếm & Lọc
        public string Keyword { get; set; }
        public int? DepartmentId { get; set; }
        public string Status { get; set; }

        // Tham số Sắp xếp: code | name | price | date ; asc | desc
        public string SortBy { get; set; } = "code";
        public string SortDir { get; set; } = "asc";

        // Tham số Phân trang (Pagination)
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 8;
        public int TotalItems { get; set; }

        public int TotalPages
        {
            get
            {
                if (PageSize <= 0) return 1;
                return Math.Max(1, (int)Math.Ceiling((double)TotalItems / PageSize));
            }
        }

        // Danh sách thiết bị của trang hiện tại
        public IList<EquipmentAsset> Items { get; set; } = new List<EquipmentAsset>();

        // DropdownList cho bộ lọc Khoa và Trạng thái
        public IEnumerable<SelectListItem> DepartmentList { get; set; }
        public IEnumerable<SelectListItem> StatusList { get; set; }
    }
}
