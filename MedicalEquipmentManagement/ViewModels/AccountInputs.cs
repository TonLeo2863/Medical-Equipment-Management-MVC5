using System.ComponentModel.DataAnnotations;

namespace MedicalEquipmentManagement.ViewModels
{
    public class NewPasswordInput
    {
        [Required, StringLength(256, MinimumLength = 12), DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; }

        [Required, Compare(nameof(NewPassword)), DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        public string Confirmation { get; set; }
    }

    public class ChangePasswordInput : NewPasswordInput
    {
        [Required, StringLength(256), DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string CurrentPassword { get; set; }
    }

    public class UserAdminInput
    {
        public int UserId { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; }

        [Required, StringLength(150)]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; }

        [EmailAddress, StringLength(150)]
        public string Email { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Vai trò")]
        public int RoleId { get; set; }

        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; }

        [StringLength(256, MinimumLength = 12), DataType(DataType.Password)]
        [Display(Name = "Mật khẩu ban đầu")]
        public string InitialPassword { get; set; }
    }
}