using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalEquipmentManagement.Models
{
    public class AssetTransfer
    {
        public AssetTransfer()
        {
            TransferDate = DateTime.Now;
            Status = "Đã chuyển";
        }

        [Key]
        [Display(Name = "Mã điều chuyển")]
        public int TransferId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thiết bị")]
        [Display(Name = "Thiết bị")]
        public int AssetId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn khoa/phòng chuyển đi")]
        [Display(Name = "Từ khoa/phòng")]
        public int FromDepartmentId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn khoa/phòng chuyển đến")]
        [Display(Name = "Đến khoa/phòng")]
        public int ToDepartmentId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày chuyển")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Ngày điều chuyển")]
        public DateTime TransferDate { get; set; }

        [StringLength(250, ErrorMessage = "Lý do không quá 250 ký tự")]
        [Display(Name = "Lý do điều chuyển")]
        public string Reason { get; set; }

        [Display(Name = "Người yêu cầu")]
        public int? RequestedBy { get; set; }

        [Display(Name = "Người duyệt")]
        public int? ApprovedBy { get; set; }

        [StringLength(50)]
        [Display(Name = "Trạng thái phiếu")]
        public string Status { get; set; }

        [ForeignKey("AssetId")]
        public virtual EquipmentAsset EquipmentAsset { get; set; }

        [ForeignKey("FromDepartmentId")]
        public virtual Department FromDepartment { get; set; }

        [ForeignKey("ToDepartmentId")]
        public virtual Department ToDepartment { get; set; }
    }
}