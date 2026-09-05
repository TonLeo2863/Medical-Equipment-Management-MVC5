using System.Data.Entity;
using MedicalEquipmentManagement.Models;

namespace MedicalEquipmentManagement.Data
{
    public class MedicalEquipmentContext : DbContext
    {
        public MedicalEquipmentContext() : base("name=MedicalEquipmentConnection") { }
        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<EquipmentCategory> EquipmentCategories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
