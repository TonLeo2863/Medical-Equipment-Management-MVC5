using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedicalEquipmentManagement.Models
{
    public class EquipmentAsset
    {
        [Key]
        public int AssetId { get; set; }

        [Required]
        [StringLength(50)]
        public string SerialNumber { get; set; }

        public int ModelId { get; set; }

        public int? DepartmentId { get; set; }

        [StringLength(50)]
        public string Status { get; set; } // Ví dụ: Đang sử dụng, Đang báo hỏng, Đã thanh lý...

        // Navigation properties
        public virtual EquipmentModel EquipmentModel { get; set; }
        public virtual Department Department { get; set; }
        public virtual ICollection<MaintenanceRecord> MaintenanceRecords { get; set; }
        public virtual ICollection<CalibrationLog> CalibrationLogs { get; set; }
    }
}