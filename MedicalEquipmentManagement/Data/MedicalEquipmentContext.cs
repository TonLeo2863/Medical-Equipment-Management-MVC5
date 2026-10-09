using System.Data.Entity;
using MedicalEquipmentManagement.Models;

namespace MedicalEquipmentManagement.Data
{
    public class MedicalEquipmentContext : DbContext
    {
        public MedicalEquipmentContext() : base("name=MedicalEquipmentConnection") { }

        // CÁC BẢNG NỀN TẢNG (Phần 1)
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<EquipmentCategory> EquipmentCategories { get; set; }
        public DbSet<EquipmentModel> EquipmentModels { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<EquipmentAsset> EquipmentAssets { get; set; }
        public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }
        public DbSet<CalibrationLog> CalibrationLogs { get; set; }

        // CÁC BẢNG QUẢN LÝ VẬN HÀNH (Phần bổ sung)
        public DbSet<WorkOrder> WorkOrders { get; set; }
        public DbSet<AssetTransfer> AssetTransfers { get; set; }
        public DbSet<SparePart> SpareParts { get; set; }
        public DbSet<UsageLog> UsageLogs { get; set; }
    }
}