using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedicalEquipmentManagement.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Tên quyền không được để trống")]
        [StringLength(50)]
        public string RoleName { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}