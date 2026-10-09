using System;
using System.ComponentModel.DataAnnotations;

namespace MedicalEquipmentManagement.Models
{
    public class Contract
    {
        public int ContractId { get; set; }
        public string ContractNumber { get; set; }
        public int SupplierId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal ContractValue { get; set; }
        public virtual Supplier Supplier { get; set; }
    }
}