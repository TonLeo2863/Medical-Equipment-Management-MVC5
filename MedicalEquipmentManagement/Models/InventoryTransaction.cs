using System;
using System.ComponentModel.DataAnnotations;
namespace MedicalEquipmentManagement.Models
{
    public class InventoryTransaction
    {
        [Key] public int TransactionId { get; set; }
        [Required] public int EquipmentId { get; set; }
        public int? DepartmentId { get; set; }
        [Required, StringLength(30)] public string TransactionType { get; set; }
        [Range(1, int.MaxValue)] public int Quantity { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Note { get; set; }
    }
}
