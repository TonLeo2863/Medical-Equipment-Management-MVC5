using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalEquipmentManagement.Models
{
    /// <summary>
    /// Thực thể Nhà cung cấp & Hãng sản xuất (Bảng Suppliers).
    /// </summary>
    [Table("Suppliers")]
    public class Supplier
    {
        public Supplier()
        {
            ManufacturedModels = new HashSet<EquipmentModel>();
            SupplierType = SupplierTypeConst.Manufacturer;
            IsActive = true;
        }

        [Key]
        [Display(Name = "Mã ID")]
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên nhà cung cấp / hãng sản xuất")]
        [StringLength(150, ErrorMessage = "Tên không vượt quá 150 ký tự")]
        [Display(Name = "Tên nhà cung cấp / Hãng SX")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phân loại")]
        [StringLength(50)]
        [Display(Name = "Phân loại")]
        public string SupplierType { get; set; }

        [StringLength(30, ErrorMessage = "Số điện thoại không vượt quá 30 ký tự")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; }

        [EmailAddress(ErrorMessage = "Địa chỉ Email không hợp lệ")]
        [StringLength(150, ErrorMessage = "Email không vượt quá 150 ký tự")]
        [Display(Name = "Email liên hệ")]
        public string Email { get; set; }

        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; }

        public virtual ICollection<EquipmentModel> ManufacturedModels { get; set; }
    }
}
