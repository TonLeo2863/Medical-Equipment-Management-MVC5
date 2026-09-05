using System.ComponentModel.DataAnnotations;
namespace MedicalEquipmentManagement.Models
{
    public class User
    {
        [Key] public int UserId { get; set; }
        [Required, StringLength(50)] public string Username { get; set; }
        [Required, StringLength(255)] public string PasswordHash { get; set; }
        [Required, StringLength(150)] public string FullName { get; set; }
        [Required, StringLength(30)] public string Role { get; set; }
        public bool IsActive { get; set; }
    }
}
