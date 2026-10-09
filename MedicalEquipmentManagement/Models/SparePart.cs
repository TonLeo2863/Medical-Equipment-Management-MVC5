using System.ComponentModel.DataAnnotations;

namespace MedicalEquipmentManagement.Models
{
    public class SparePart
    {
        [Key]
        public int PartId { get; set; }

        [Required]
        [StringLength(100)]
        public string PartName { get; set; }

        public int QuantityInStock { get; set; }

        public decimal Price { get; set; }

        public int? SupplierId { get; set; }

        // Navigation property
        public virtual Supplier Supplier { get; set; }
    }
}