using System;
using System.ComponentModel.DataAnnotations;

namespace MedicalEquipmentManagement.Models
{
    public class CalibrationLog
    {
        public int CalibrationId { get; set; }
        public int AssetId { get; set; }
        public DateTime CalibrationDate { get; set; }
        public DateTime? NextCalibrationDate { get; set; }
        public string Result { get; set; }
        public string PerformedBy { get; set; }
        public virtual EquipmentAsset EquipmentAsset { get; set; }
    }
}