using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalEquipmentManagement.Models
{
    [Table("EquipmentModels")]
    public class EquipmentModel
    {
        [Key]
        public int ModelId { get; set; }

        [Column("Name")]
        public string ModelName { get; set; }

        [NotMapped]
        public string Manufacturer { get; set; }

        public int? CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual EquipmentCategory Category { get; set; }

        public virtual ICollection<EquipmentAsset> EquipmentAssets { get; set; }
    }
}