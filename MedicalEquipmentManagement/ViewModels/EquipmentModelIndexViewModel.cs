using System.Collections.Generic;
using System.Web.Mvc;
using MedicalEquipmentManagement.Models;

namespace MedicalEquipmentManagement.ViewModels
{
    /// <summary>
    /// ViewModel cho trang Danh sách Mẫu thiết bị (EquipmentModel/Index):
    /// đóng gói từ khóa tìm kiếm, bộ lọc Danh mục và DropdownList (không dùng ViewBag).
    /// </summary>
    public class EquipmentModelIndexViewModel
    {
        public string Keyword { get; set; }
        public int? CategoryId { get; set; }

        public IList<EquipmentModel> Items { get; set; } = new List<EquipmentModel>();

        // DropdownList lọc theo Danh mục (Value = ID, Text = Tên)
        public IEnumerable<SelectListItem> CategoryList { get; set; }
    }
}
