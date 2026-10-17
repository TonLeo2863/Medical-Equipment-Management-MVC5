using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalEquipmentManagement.Models
{
    public class MaintenanceRecord
    {
        [Key]
        [Display(Name = "Mã bảo trì")]
        public int MaintenanceId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thiết bị")]
        [Display(Name = "Thiết bị")]
        public int AssetId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hoặc nhập loại bảo trì")]
        [StringLength(50, ErrorMessage = "Loại bảo trì không quá 50 ký tự")]
        [Display(Name = "Loại bảo trì")]
        public string MaintenanceType { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày thực hiện")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Ngày bảo trì")]
        public DateTime MaintenanceDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Ngày bảo trì kế tiếp")]
        public DateTime? NextMaintenanceDate { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả không quá 500 ký tự")]
        [Display(Name = "Mô tả / Ghi chú")]
        public string Description { get; set; }

        [Range(0, 10000000000, ErrorMessage = "Chi phí phải từ 0 trở lên")]
        [Display(Name = "Chi phí (VNĐ)")]
        public decimal? Cost { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập kết quả xử lý")]
        [StringLength(100, ErrorMessage = "Kết quả không quá 100 ký tự")]
        [Display(Name = "Kết quả xử lý")]
        public string ResultStatus { get; set; }

        [NotMapped]
        [Display(Name = "Mã sự cố (nếu có)")]
        public int? TicketId { get; set; }

        [ForeignKey("AssetId")]
        public virtual EquipmentAsset EquipmentAsset { get; set; }
    }
}
