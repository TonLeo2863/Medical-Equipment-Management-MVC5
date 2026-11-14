using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalEquipmentManagement.Models
{
    /// <summary>
    /// Thực thể Danh mục loại thiết bị y tế (Bảng EquipmentCategories).
    /// </summary>
    [Table("EquipmentCategories")]
    public class EquipmentCategory
    {
        public EquipmentCategory()
        {
            EquipmentModels = new HashSet<EquipmentModel>();
            IsActive = true;
        }

        [Key]
        [Display(Name = "Mã ID")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã danh mục")]
        [StringLength(20, ErrorMessage = "Mã danh mục không vượt quá 20 ký tự")]
        [Display(Name = "Mã danh mục")]
        public string CategoryCode { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên danh mục thiết bị")]
        [StringLength(150, ErrorMessage = "Tên danh mục không vượt quá 150 ký tự")]
        [Display(Name = "Tên danh mục")]
        public string Name { get; set; }

        [Display(Name = "Đang sử dụng")]
        public bool IsActive { get; set; }

        public virtual ICollection<EquipmentModel> EquipmentModels { get; set; }
    }
}