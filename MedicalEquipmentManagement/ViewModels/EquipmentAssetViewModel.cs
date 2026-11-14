using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using MedicalEquipmentManagement.Models;

namespace MedicalEquipmentManagement.ViewModels
{
    /// <summary>
    /// ViewModel cho Form Thêm mới / Chỉnh sửa Tài sản thiết bị thực tế (EquipmentAsset).
    /// Bắt buộc theo yêu cầu đề bài: Dùng ViewModel để truyền dữ liệu DropdownList
    /// (chọn Model và Department) ra View, không dùng ViewBag lộn xộn.
    /// </summary>
    public class EquipmentAssetViewModel
    {
        public int AssetId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã tài sản (Asset Code)")]
        [StringLength(50, ErrorMessage = "Mã tài sản không vượt quá 50 ký tự")]
        [Display(Name = "Mã tài sản (Asset Code)")]
        public string AssetCode { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số Serial của thiết bị")]
        [StringLength(100, ErrorMessage = "Số Serial không vượt quá 100 ký tự")]
        [Display(Name = "Số Serial (Serial Number)")]
        public string SerialNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn mẫu thiết bị (Model)")]
        [Display(Name = "Mẫu thiết bị (Model)")]
        public int? ModelId { get; set; }

        [Display(Name = "Khoa / Phòng sử dụng")]
        public int? DepartmentId { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Ngày mua")]
        public DateTime? PurchaseDate { get; set; }

        [Range(0, 1000000000000, ErrorMessage = "Giá mua phải lớn hơn hoặc bằng 0")]
        [Display(Name = "Giá mua (VNĐ)")]
        public decimal? PurchasePrice { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trạng thái thiết bị")]
        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = AssetStatus.Active;

        // Các DropdownList truyền ra View (Value = ID, Text = Tên hiển thị)
        public IEnumerable<SelectListItem> ModelList { get; set; }
        public IEnumerable<SelectListItem> DepartmentList { get; set; }
        public IEnumerable<SelectListItem> StatusList { get; set; }

        public static EquipmentAssetViewModel FromEntity(EquipmentAsset asset)
        {
            return new EquipmentAssetViewModel
            {
                AssetId = asset.AssetId,
                AssetCode = asset.AssetCode,
                SerialNumber = asset.SerialNumber,
                ModelId = asset.ModelId,
                DepartmentId = asset.DepartmentId,
                PurchaseDate = asset.PurchaseDate,
                PurchasePrice = asset.PurchasePrice,
                Status = string.IsNullOrWhiteSpace(asset.Status) ? AssetStatus.Active : asset.Status
            };
        }

        public void ApplyToEntity(EquipmentAsset asset)
        {
            asset.AssetCode = AssetCode != null ? AssetCode.Trim().ToUpperInvariant() : null;
            asset.SerialNumber = SerialNumber != null ? SerialNumber.Trim() : null;
            asset.ModelId = ModelId ?? 0;
            asset.DepartmentId = DepartmentId;
            asset.PurchaseDate = PurchaseDate;
            asset.PurchasePrice = PurchasePrice;
            asset.Status = string.IsNullOrWhiteSpace(Status) ? AssetStatus.Active : Status.Trim();
        }
    }
}
