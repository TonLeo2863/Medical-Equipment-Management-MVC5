using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalEquipmentManagement.Models
{
    [Table("Suppliers")]
    public class Supplier
    {
        [Key]
        public int SupplierId { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        [NotMapped]
        public string Address { get; set; }
    }
}
