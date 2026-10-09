using System;
using System.ComponentModel.DataAnnotations;

namespace MedicalEquipmentManagement.Models
{
    public class WorkOrder
    {
        [Key]
        public int WorkOrderId { get; set; }

        public int AssetId { get; set; }

        public string IssueDescription { get; set; }

        public DateTime CreatedDate { get; set; }

        [StringLength(50)]
        public string Status { get; set; } // Ví dụ: Chờ xử lý, Đang sửa, Hoàn thành

        public int? AssignedToUserId { get; set; }

        // Navigation properties
        public virtual EquipmentAsset EquipmentAsset { get; set; }
        public virtual User AssignedUser { get; set; }
    }
}