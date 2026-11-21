using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalEquipmentManagement.Models
{
    public class CalibrationLog
    {
        public CalibrationLog()
        {
            CalibrationDate = DateTime.Today;
            Result = "Đạt";
        }

        [Key]
        [Display(Name = "Mã kiểm định")]
        public int CalibrationId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thiết bị")]
        [Display(Name = "Thiết bị")]
        public int AssetId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày kiểm định")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Ngày kiểm định")]
        public DateTime CalibrationDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Hạn kiểm định tiếp theo")]
        public DateTime? NextCalibrationDate { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập kết quả")]
        [StringLength(50)]
        [Display(Name = "Kết quả kiểm định")]
        public string Result { get; set; } // Đạt / Không đạt

        [Required(ErrorMessage = "Vui lòng nhập đơn vị hoặc người thực hiện")]
        [StringLength(150, ErrorMessage = "Tên đơn vị không quá 150 ký tự")]
        [Display(Name = "Đơn vị / Người thực hiện")]
        public string PerformedBy { get; set; }

        [StringLength(100)]
        [Display(Name = "Số giấy chứng nhận")]
        public string CertificateNumber { get; set; }

        [ForeignKey("AssetId")]
        public virtual EquipmentAsset EquipmentAsset { get; set; }
    }
}