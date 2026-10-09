using System;
using System.ComponentModel.DataAnnotations;

namespace MedicalEquipmentManagement.Models
{
    public class UsageLog
    {
        public int LogId { get; set; }
        public int AssetId { get; set; }
        public int UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Notes { get; set; }
        public virtual EquipmentAsset EquipmentAsset { get; set; }
        public virtual User User { get; set; }
    }
}