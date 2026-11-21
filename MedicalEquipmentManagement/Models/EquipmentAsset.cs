using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalEquipmentManagement.Models
{
    [Table("EquipmentAssets")]
    public class EquipmentAsset
    {
        [Key]
        public int AssetId { get; set; }

        public string AssetCode { get; set; }

        public string SerialNumber { get; set; }

        public int ModelId { get; set; }

        public int? DepartmentId { get; set; }

        public string Status { get; set; }

        [ForeignKey("ModelId")]
        public virtual EquipmentModel EquipmentModel { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }

        public virtual ICollection<MaintenanceRecord> MaintenanceRecords { get; set; }

        public virtual ICollection<CalibrationLog> CalibrationLogs { get; set; }
    }
}