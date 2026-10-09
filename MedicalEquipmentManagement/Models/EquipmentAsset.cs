using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedicalEquipmentManagement.Models
{
    public class EquipmentAsset
    {
        public int AssetId { get; set; }
        public string SerialNumber { get; set; }
        public int ModelId { get; set; }
        public int? DepartmentId { get; set; }
        public string Status { get; set; }
        public virtual EquipmentModel EquipmentModel { get; set; }
        public virtual Department Department { get; set; }
        public virtual ICollection<MaintenanceRecord> MaintenanceRecords { get; set; }
        public virtual ICollection<CalibrationLog> CalibrationLogs { get; set; }
    }
}