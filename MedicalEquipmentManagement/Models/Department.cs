using System.ComponentModel.DataAnnotations;
namespace MedicalEquipmentManagement.Models
{
    public class Department
    {
        [Key] public int DepartmentId { get; set; }
        [Required, StringLength(150)] public string Name { get; set; }
        [StringLength(250)] public string Location { get; set; }
    }
}
