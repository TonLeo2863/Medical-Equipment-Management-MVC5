using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalEquipmentManagement.Models
{
    /// <summary>
    /// Thực thể Mẫu thiết bị (Bảng EquipmentModels).
    /// Quản lý thông tin chung của mẫu máy, hãng sản xuất và phân loại danh mục (Category).
    /// </summary>
    [Table("EquipmentModels")]
    public class EquipmentModel
    {
        public EquipmentModel()
        {
            EquipmentAssets = new HashSet<EquipmentAsset>();
            IsActive = true;
        }

        [Key]
        [Display(Name = "Mã ID")]
        public int ModelId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã mẫu thiết bị (Model Code)")]
        [StringLength(50, ErrorMessage = "Mã mẫu thiết bị không vượt quá 50 ký tự")]
        [Display(Name = "Mã Model")]
        public string ModelCode { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên mẫu thiết bị")]
        [StringLength(200, ErrorMessage = "Tên mẫu thiết bị không vượt quá 200 ký tự")]
        [Column("Name")]
        [Display(Name = "Tên mẫu thiết bị")]
        public string ModelName { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục loại thiết bị")]
        [Display(Name = "Danh mục thiết bị")]
        public int? CategoryId { get; set; }

        [Display(Name = "Hãng sản xuất")]
        public int? ManufacturerId { get; set; }

        [Display(Name = "Đang lưu hành")]
        public bool IsActive { get; set; }

        [ForeignKey("CategoryId")]
        public virtual EquipmentCategory Category { get; set; }

        [ForeignKey("ManufacturerId")]
        public virtual Supplier ManufacturerSupplier { get; set; }

        public virtual ICollection<EquipmentAsset> EquipmentAssets { get; set; }
    }
}