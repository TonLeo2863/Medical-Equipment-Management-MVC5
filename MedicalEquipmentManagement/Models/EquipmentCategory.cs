using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalEquipmentManagement.Models
{
    [Table("EquipmentCategories")]
    public class EquipmentCategory
    {
        [Key]
        public int CategoryId { get; set; }

        public string Name { get; set; }

        [NotMapped]
        public string Description { get; set; }

        public virtual ICollection<EquipmentModel> EquipmentModels { get; set; }
    }
}