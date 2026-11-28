using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalEquipmentManagement.Models
{
    /// <summary>Phiếu sửa chữa của một tài sản. Người báo, kỹ sư được giao và người hoàn tất được lưu riêng.</summary>
    [Table("WorkOrders")]
    public class WorkOrder
    {
        [Key]
        [Column("TicketId")]
        [Display(Name = "Mã phiếu sửa chữa")]
        public int WorkOrderId { get; set; }

        [Column("TicketCode")]
        [Display(Name = "Mã số phiếu")]
        public string TicketCode { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thiết bị")]
        [Display(Name = "Thiết bị")]
        public int AssetId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả sự cố")]
        [StringLength(500, ErrorMessage = "Mô tả sự cố không vượt quá 500 ký tự")]
        [Display(Name = "Mô tả sự cố")]
        public string IssueDescription { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày tạo phiếu")]
        [Column("ReportedDate")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Ngày báo sự cố")]
        public DateTime CreatedDate { get; set; }

        [StringLength(20)]
        [Display(Name = "Mức độ khẩn")]
        public string UrgencyLevel { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Trạng thái phiếu")]
        public string Status { get; set; } // Chờ xử lý, Đang sửa, Đã xong

        [Column("ReportedBy")]
        public int ReportedByUserId { get; set; }

        public bool ReporterVerified { get; set; }

        [Display(Name = "Kỹ sư được phân công")]
        public int? AssignedToUserId { get; set; }

        [StringLength(500)]
        public string Resolution { get; set; }

        public bool? TechnicalPassed { get; set; }

        public int? ResolvedByUserId { get; set; }

        [Column("ResolvedDate")]
        [Display(Name = "Ngày xử lý xong")]
        public DateTime? ResolvedDate { get; set; }

        [ForeignKey("AssetId")]
        public virtual EquipmentAsset EquipmentAsset { get; set; }

        [ForeignKey("AssignedToUserId")]
        public virtual User AssignedUser { get; set; }

        [ForeignKey("ReportedByUserId")]
        public virtual User Reporter { get; set; }

        [ForeignKey("ResolvedByUserId")]
        public virtual User ResolvedBy { get; set; }
    }

    public static class WorkOrderStatus
    {
        public const string Pending = "Chờ xử lý";
        public const string InProgress = "Đang sửa";
        public const string Completed = "Đã xong";
        public const string Cancelled = "Đã hủy";
    }
}
