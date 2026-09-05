using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
        public virtual ICollection<Equipment> Equipments { get; set; }
    }
}
