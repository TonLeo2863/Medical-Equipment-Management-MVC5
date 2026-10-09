using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MedicalEquipmentManagement.Models; // Add this if Equipment is in the same namespace
// OR
// using MedicalEquipmentManagement.Models.EquipmentNamespace; // Use the correct namespace if Equipment is in a sub-namespace

namespace MedicalEquipmentManagement.Models
{
    public class EquipmentCategory
    {
        [Key]
        public int CategoryId { get; set; }
        [Required, StringLength(150)]
        public string Name { get; set; }
        [StringLength(500)]
        public string Description { get; set; }
        public virtual ICollection<EquipmentModel> EquipmentModels { get; set; }
    }
}