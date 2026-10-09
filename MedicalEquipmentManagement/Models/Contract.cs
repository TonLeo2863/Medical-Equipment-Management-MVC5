using System;
using System.ComponentModel.DataAnnotations;

namespace MedicalEquipmentManagement.Models
{
    public class Contract
    {
        [Key]
        public int ContractId { get; set; }

        [Required]
        [StringLength(100)]
        public string ContractNumber { get; set; }

        public int SupplierId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public decimal ContractValue { get; set; }

        // Navigation property
        public virtual Supplier Supplier { get; set; }
    }
}