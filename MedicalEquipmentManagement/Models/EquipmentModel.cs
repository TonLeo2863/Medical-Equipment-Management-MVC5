using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedicalEquipmentManagement.Models
{
    public class EquipmentModel
    {
        public int ModelId { get; set; }
        public string ModelName { get; set; }
        public string Manufacturer { get; set; }
        public int? CategoryId { get; set; }
        public virtual EquipmentCategory Category { get; set; }
        public virtual ICollection<EquipmentAsset> EquipmentAssets { get; set; }
    }
}