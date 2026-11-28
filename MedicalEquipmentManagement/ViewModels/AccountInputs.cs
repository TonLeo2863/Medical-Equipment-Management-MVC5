using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace MedicalEquipmentManagement.ViewModels
{
    /// <summary>Giữ tên trường đăng nhập. Giá trị mật khẩu đi thẳng tới PasswordHasher nên chỉ trường này được phép chứa văn bản giống HTML.</summary>
    public class LoginInput
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập."), StringLength(50, ErrorMessage = "Tên đăng nhập tối đa 50 ký tự.")]
        public string Username { get; set; }

        [AllowHtml, Required(ErrorMessage = "Vui lòng nhập mật khẩu."), StringLength(256, ErrorMessage = "Mật khẩu tối đa 256 ký tự.")]
        public string Password { get; set; }
    }

    /// <summary>Áp dụng cùng quy tắc độ dài và xác nhận cho mọi mật khẩu mới. Razor không hiển thị lại giá trị mật khẩu.</summary>
    public class NewPasswordInput
    {
        [AllowHtml, Required(ErrorMessage = "Vui lòng nhập mật khẩu mới."), StringLength(256, MinimumLength = 12, ErrorMessage = "Mật khẩu mới phải dài từ 12 đến 256 ký tự."), DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; }

        [AllowHtml, Required(ErrorMessage = "Vui lòng xác nhận mật khẩu."), System.ComponentModel.DataAnnotations.Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp."), DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        public string Confirmation { get; set; }
    }

    public class ChangePasswordInput : NewPasswordInput
    {
        [AllowHtml, Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại."), StringLength(256, ErrorMessage = "Mật khẩu hiện tại tối đa 256 ký tự."), DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string CurrentPassword { get; set; }
    }

    /// <summary>Giới hạn trường được quản trị. Hai trường mật khẩu ban đầu chỉ được kiểm tra khi tạo tài khoản.</summary>
    public class UserAdminInput
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập."), StringLength(50, ErrorMessage = "Tên đăng nhập tối đa 50 ký tự.")]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên."), StringLength(150, ErrorMessage = "Họ tên tối đa 150 ký tự.")]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ."), StringLength(150, ErrorMessage = "Email tối đa 150 ký tự.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò."), Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn vai trò.")]
        [Display(Name = "Vai trò")]
        public int? RoleId { get; set; }

        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; }

        [AllowHtml, StringLength(256, MinimumLength = 12, ErrorMessage = "Mật khẩu ban đầu phải dài từ 12 đến 256 ký tự."), DataType(DataType.Password)]
        [Display(Name = "Mật khẩu ban đầu")]
        public string InitialPassword { get; set; }

        [AllowHtml, DataType(DataType.Password), StringLength(256, ErrorMessage = "Xác nhận mật khẩu tối đa 256 ký tự.")]
        [Display(Name = "Xác nhận mật khẩu")]
        public string InitialConfirmation { get; set; }
    }
}
