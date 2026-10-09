using System;
using System.ComponentModel.DataAnnotations;

namespace MedicalEquipmentManagement.Models
{
    public class WorkOrder
    {
        public int WorkOrderId { get; set; }
        public int AssetId { get; set; }
        public string IssueDescription { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; } 
        public int? AssignedToUserId { get; set; }
        public virtual EquipmentAsset EquipmentAsset { get; set; }
        public virtual User AssignedUser { get; set; }
    }
}