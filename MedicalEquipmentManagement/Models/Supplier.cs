using System.ComponentModel.DataAnnotations;
namespace MedicalEquipmentManagement.Models
{
    public class Supplier
    {
        [Key] public int SupplierId { get; set; }
        [Required, StringLength(150)] public string Name { get; set; }
        [StringLength(30)] public string Phone { get; set; }
        [EmailAddress, StringLength(150)] public string Email { get; set; }
        [StringLength(250)] public string Address { get; set; }
    }
}
