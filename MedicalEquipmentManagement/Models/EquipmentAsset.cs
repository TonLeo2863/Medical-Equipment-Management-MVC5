using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalEquipmentManagement.Models
{
    /// <summary>
    /// Thực thể Tài sản thiết bị thực tế (Bảng EquipmentAssets).
    /// Đại diện cho từng thiết bị vật lý cụ thể (Số Serial, Mã tài sản, Trạng thái, Khoa/Phòng).
    /// </summary>
    [Table("EquipmentAssets")]
    public class EquipmentAsset
    {
        public EquipmentAsset()
        {
            MaintenanceRecords = new HashSet<MaintenanceRecord>();
            Status = AssetStatus.Active;
            IsDeleted = false;
        }

        [Key]
        [Display(Name = "ID Tài sản")]
        public int AssetId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã tài sản (Asset Code)")]
        [StringLength(50, ErrorMessage = "Mã tài sản không vượt quá 50 ký tự")]
        [Display(Name = "Mã tài sản")]
        public string AssetCode { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số Serial của thiết bị")]
        [StringLength(100, ErrorMessage = "Số Serial không vượt quá 100 ký tự")]
        [Display(Name = "Số Serial")]
        public string SerialNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn mẫu thiết bị")]
        [Display(Name = "Mẫu thiết bị")]
        public int ModelId { get; set; }

        [Display(Name = "Khoa / Phòng sử dụng")]
        public int? DepartmentId { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Ngày mua")]
        public DateTime? PurchaseDate { get; set; }

        [Range(0, 1000000000000, ErrorMessage = "Giá mua phải lớn hơn hoặc bằng 0")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(Name = "Giá mua (VNĐ)")]
        public decimal? PurchasePrice { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trạng thái")]
        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; }

        public bool IsDeleted { get; set; }

        [ForeignKey("ModelId")]
        public virtual EquipmentModel EquipmentModel { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }

        public virtual ICollection<MaintenanceRecord> MaintenanceRecords { get; set; }
    }
}