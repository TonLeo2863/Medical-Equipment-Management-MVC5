using System.ComponentModel.DataAnnotations;
namespace MedicalEquipmentManagement.Models
{
    public class Department
    {
        public int DepartmentId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
    }
}
