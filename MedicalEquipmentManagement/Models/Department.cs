using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedicalEquipmentManagement.Models
{
    public class Department
    {
        public Department()
        {
            EquipmentAssets = new HashSet<EquipmentAsset>();
            IsActive = true;
        }

        [Key]
        [Display(Name = "ID")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã khoa/phòng")]
        [StringLength(20, ErrorMessage = "Mã phòng không quá 20 ký tự")]
        [Display(Name = "Mã khoa/phòng")]
        public string DepartmentCode { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên khoa/phòng")]
        [StringLength(150, ErrorMessage = "Tên khoa/phòng không quá 150 ký tự")]
        [Display(Name = "Tên khoa/phòng")]
        public string Name { get; set; }

        [StringLength(250, ErrorMessage = "Vị trí không quá 250 ký tự")]
        [Display(Name = "Vị trí / Tòa nhà")]
        public string Location { get; set; }

        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; }

        public virtual ICollection<EquipmentAsset> EquipmentAssets { get; set; }
    }
}
