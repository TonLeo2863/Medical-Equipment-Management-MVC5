using System;
using System.ComponentModel.DataAnnotations;
namespace MedicalEquipmentManagement.Models
{
    public class MaintenanceRecord
    {
        public int MaintenanceId { get; set; }
        public int EquipmentId { get; set; }
        public DateTime MaintenanceDate { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }
        public string Description { get; set; }
        public decimal? Cost { get; set; }
        public string Result { get; set; }
    }
}
