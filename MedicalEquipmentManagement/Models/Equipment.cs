using System;
using System.ComponentModel.DataAnnotations;

namespace MedicalEquipmentManagement.Models
{
    public class Equipment
    {
        [Key]
        public int EquipmentId { get; set; }
        [Required, StringLength(50)]
        public string EquipmentCode { get; set; }
        [Required, StringLength(200)]
        public string Name { get; set; }
        [Required]
        public int CategoryId { get; set; }
        public int? SupplierId { get; set; }
        public int? DepartmentId { get; set; }
        public DateTime? PurchaseDate { get; set; }
        [Range(0, double.MaxValue)]
        public decimal? PurchasePrice { get; set; }
        [Required, StringLength(50)]
        public string Status { get; set; }
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
        public string ImagePath { get; set; }
        [StringLength(1000)]
        public string Description { get; set; }
        public virtual EquipmentCategory Category { get; set; }
        public virtual Supplier Supplier { get; set; }
        public virtual Department Department { get; set; }
    }
}
