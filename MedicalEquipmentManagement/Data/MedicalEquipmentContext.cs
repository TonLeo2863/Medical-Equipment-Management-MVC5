using MedicalEquipmentManagement.Models;
using System.Data.Entity;
using System.Linq;

namespace MedicalEquipmentManagement.Data

{
    /// <summary>Ánh xạ EF6 sang schema SQL có sẵn. Tắt xóa dây chuyền để giữ dữ liệu nghiệp vụ liên quan.</summary>
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
        public DbSet<CalibrationLog> CalibrationLogs { get; set; }
        public DbSet<WorkOrder> WorkOrders { get; set; }
        public DbSet<AssetTransfer> AssetTransfers { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<SparePart> SpareParts { get; set; }
        public DbSet<UsageLog> UsageLogs { get; set; }

        /// <summary>Khóa dòng tài sản trong transaction hiện tại để tuần tự hóa cập nhật vị trí và trạng thái nghiệp vụ.</summary>
        /// <param name="assetId">Khóa chính của tài sản cần khóa trước khi đọc dữ liệu để quyết định cập nhật.</param>
        public void LockAsset(int assetId)
        {
            Database.SqlQuery<int>("SELECT AssetId FROM dbo.EquipmentAssets WITH (UPDLOCK, HOLDLOCK) WHERE AssetId = @p0",
                assetId).ToList();
        }

        /// <summary>Truy vấn bản ghi bảo trì mới nhất của từng thiết bị. Mã bản ghi phân định các lần cùng ngày.</summary>
        public IQueryable<MaintenanceRecord> LatestMaintenance()
        {
            return MaintenanceRecords.Where(m => !MaintenanceRecords.Any(n => n.AssetId == m.AssetId
                && (n.MaintenanceDate > m.MaintenanceDate
                    || n.MaintenanceDate == m.MaintenanceDate && n.MaintenanceId > m.MaintenanceId)));
        }

        /// <summary>Truy vấn lần hiệu chuẩn mới nhất để đánh giá hạn và kết quả kỹ thuật hiện hành.</summary>
        public IQueryable<CalibrationLog> LatestCalibrations()
        {
            return CalibrationLogs.Where(c => !CalibrationLogs.Any(n => n.AssetId == c.AssetId
                && (n.CalibrationDate > c.CalibrationDate
                    || n.CalibrationDate == c.CalibrationDate && n.CalibrationId > c.CalibrationId)));
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Conventions.Remove<System.Data.Entity.ModelConfiguration.Conventions.OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<System.Data.Entity.ModelConfiguration.Conventions.ManyToManyCascadeDeleteConvention>();
        }
    }

}
