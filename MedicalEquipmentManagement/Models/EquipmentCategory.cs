using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MedicalEquipmentManagement.Models;
namespace MedicalEquipmentManagement.Models
{
    public class EquipmentCategory
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public virtual ICollection<EquipmentModel> EquipmentModels { get; set; }
    }
}