using MedicalEquipmentManagement.Models;
using System.Data.Entity;

namespace MedicalEquipmentManagement.Data

{
    public class MedicalEquipmentContext : DbContext

    {
        public MedicalEquipmentContext() : base("name=MedicalEquipmentConnection") { }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<EquipmentCategory> EquipmentCategories { get; set; }
        public DbSet<EquipmentModel> EquipmentModels { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<EquipmentAsset> EquipmentAssets { get; set; }
        public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }
        public DbSet<WorkOrder> WorkOrders { get; set; }
        // Lưu ý: CalibrationLog / AssetTransfer / SparePart / UsageLog chưa có khóa chính ở nhánh develop
        // (thuộc phạm vi Module 3) nên tạm thời chưa khai báo DbSet để EF khởi tạo được model.
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Conventions.Remove<System.Data.Entity.ModelConfiguration.Conventions.OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<System.Data.Entity.ModelConfiguration.Conventions.ManyToManyCascadeDeleteConvention>();
        }
    }

}