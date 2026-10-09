using System.ComponentModel.DataAnnotations;

namespace MedicalEquipmentManagement.Models
{
    public class SparePart
    {
        public int PartId { get; set; }
        public string PartName { get; set; }
        public int QuantityInStock { get; set; }
        public decimal Price { get; set; }
        public int? SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; }
    }
}