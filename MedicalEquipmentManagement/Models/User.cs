using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalEquipmentManagement.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public int RoleId { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }

        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }
    }
}
