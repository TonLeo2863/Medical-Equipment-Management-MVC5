using System;
using System.ComponentModel.DataAnnotations;

namespace MedicalEquipmentManagement.Models
{
    public class AssetTransfer
    {
        [Key]
        public int TransferId { get; set; }

        public int AssetId { get; set; }

        public int FromDepartmentId { get; set; }

        public int ToDepartmentId { get; set; }

        public DateTime TransferDate { get; set; }

        public string Reason { get; set; }

        // Navigation properties
        public virtual EquipmentAsset EquipmentAsset { get; set; }
        public virtual Department FromDepartment { get; set; }
        public virtual Department ToDepartment { get; set; }
    }
}