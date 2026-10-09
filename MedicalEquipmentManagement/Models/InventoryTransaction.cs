using System;
using System.ComponentModel.DataAnnotations;
namespace MedicalEquipmentManagement.Models
{
    public class InventoryTransaction
    {
        public int TransactionId { get; set; }
        public int EquipmentId { get; set; }
        public int? DepartmentId { get; set; }
        public string TransactionType { get; set; }
        public int Quantity { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Note { get; set; }
    }
}
