using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using MedicalEquipmentManagement.Models;

namespace MedicalEquipmentManagement.ViewModels
{
    /// <summary>
    /// ViewModel cho Form Thêm mới / Chỉnh sửa Mẫu thiết bị (EquipmentModel).
    /// Bắt buộc theo yêu cầu đề bài: Truyền dữ liệu DropdownList (Category & Manufacturer) qua ViewModel,
    /// không dùng ViewBag lộn xộn.
    /// </summary>
    public class EquipmentModelFormViewModel
    {
        public int ModelId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã mẫu thiết bị (Model Code)")]
        [StringLength(50, ErrorMessage = "Mã mẫu thiết bị không vượt quá 50 ký tự")]
        [Display(Name = "Mã Model (Model Code)")]
        public string ModelCode { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên mẫu thiết bị")]
        [StringLength(200, ErrorMessage = "Tên mẫu thiết bị không vượt quá 200 ký tự")]
        [Display(Name = "Tên mẫu thiết bị")]
        public string ModelName { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục loại thiết bị")]
        [Display(Name = "Danh mục loại thiết bị")]
        public int? CategoryId { get; set; }

        [Display(Name = "Hãng sản xuất (Manufacturer)")]
        public int? ManufacturerId { get; set; }

        [Display(Name = "Đang lưu hành")]
        public bool IsActive { get; set; } = true;

        // Danh sách DropdownList truyền ra View (hiển thị đúng Tên chứ không hiển thị ID)
        public IEnumerable<SelectListItem> CategoryList { get; set; }
        public IEnumerable<SelectListItem> ManufacturerList { get; set; }

        /// <summary>
        /// Tạo ViewModel từ Entity EquipmentModel (phục vụ màn hình Edit).
        /// </summary>
        public static EquipmentModelFormViewModel FromEntity(EquipmentModel entity)
        {
            return new EquipmentModelFormViewModel
            {
                ModelId = entity.ModelId,
                ModelCode = entity.ModelCode,
                ModelName = entity.ModelName,
                CategoryId = entity.CategoryId,
                ManufacturerId = entity.ManufacturerId,
                IsActive = entity.IsActive
            };
        }

        /// <summary>
        /// Áp dụng dữ liệu đã nhập từ ViewModel vào Entity EquipmentModel để lưu xuống Database.
        /// </summary>
        public void ApplyToEntity(EquipmentModel entity)
        {
            entity.ModelCode = ModelCode != null ? ModelCode.Trim().ToUpperInvariant() : null;
            entity.ModelName = ModelName != null ? ModelName.Trim() : null;
            entity.CategoryId = CategoryId;
            entity.ManufacturerId = ManufacturerId;
            entity.IsActive = IsActive;
        }
    }
}
