using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using MedicalEquipmentManagement.Data;
using MedicalEquipmentManagement.Filters;
using MedicalEquipmentManagement.Helpers;
using MedicalEquipmentManagement.Models;
using MedicalEquipmentManagement.ViewModels;

namespace MedicalEquipmentManagement.Controllers
{
    /// <summary>Cho phép ADMIN tạo, cập nhật, vô hiệu hóa và đặt lại mật khẩu tài khoản.</summary>
    [CustomAuthorize(Roles = "ADMIN")]
    public class UserAdminController : Controller
    {
        private readonly MedicalEquipmentContext db = new MedicalEquipmentContext();

        public ActionResult Index()
        {
            return View(db.Users.Include(u => u.Role).OrderBy(u => u.Username).ToList());
        }

        [HttpGet]
        public ActionResult Edit(int? id)
        {
            var account = id.GetValueOrDefault() != 0 ? db.Users.Find(id.Value) : new User { IsActive = true };
            if (account == null)
                return HttpNotFound();
            return EditView(new UserAdminInput
            {
                UserId = account.UserId, Username = account.Username, FullName = account.FullName,
                Email = account.Email, RoleId = account.RoleId, IsActive = account.IsActive
            });
        }

        /// <summary>Kiểm tra mật khẩu xác nhận khi tạo. Tuần tự hóa thay đổi tài khoản để bảo đảm còn ít nhất một ADMIN hoạt động.</summary>
        /// <param name="id">ID khác 0 dùng để sửa tài khoản hiện có.</param>
        /// <param name="input">Thông tin biểu mẫu; mật khẩu ban đầu được băm trước khi ghi vào SQL.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? id, UserAdminInput input)
        {
            var userId = id.GetValueOrDefault();
            input.UserId = userId;
            if (userId == 0 && string.IsNullOrEmpty(input.InitialPassword))
                ModelState.AddModelError(nameof(input.InitialPassword), "Vui lòng nhập mật khẩu ban đầu.");
            if (userId == 0 && (string.IsNullOrEmpty(input.InitialConfirmation) || input.InitialPassword != input.InitialConfirmation))
                ModelState.AddModelError(nameof(input.InitialConfirmation), "Vui lòng xác nhận chính xác mật khẩu ban đầu.");
            if (!ModelState.IsValid)
                return EditView(input);

            using (var transaction = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                // Khóa theo transaction để hai quản trị viên không thể đồng thời loại bỏ các tài khoản quản trị cuối cùng.
                var lockResult = db.Database.SqlQuery<int>(
                    "DECLARE @result int; EXEC @result = sp_getapplock @Resource = N'AccountAdministration', @LockMode = N'Exclusive', @LockOwner = N'Transaction', @LockTimeout = 5000; SELECT @result;")
                    .Single();
                if (lockResult < 0)
                {
                    ModelState.AddModelError("", "Hệ thống đang xử lý thay đổi tài khoản khác. Vui lòng thử lại.");
                    return EditView(input);
                }

                var account = userId == 0 ? new User { CreatedAt = DateTime.Now } : db.Users.Find(userId);
                if (account == null)
                    return HttpNotFound();
                var role = db.Roles.Find(input.RoleId.Value);
                if (role == null)
                    ModelState.AddModelError(nameof(input.RoleId), "Vai trò không tồn tại.");
                if (db.Users.Any(u => u.UserId != userId && u.Username == input.Username.Trim()))
                    ModelState.AddModelError(nameof(input.Username), "Tên đăng nhập đã tồn tại.");
                if (account.IsActive && account.Role?.RoleCode == "ADMIN" && (!input.IsActive || role?.RoleCode != "ADMIN")
                    && !db.Users.Any(u => u.UserId != userId && u.IsActive && u.Role.RoleCode == "ADMIN"))
                    ModelState.AddModelError("", "Hệ thống cần tối thiểu một quản trị viên hoạt động.");
                if (!ModelState.IsValid)
                    return EditView(input);

                account.Username = input.Username.Trim();
                account.FullName = input.FullName.Trim();
                account.Email = input.Email?.Trim();
                account.RoleId = role.RoleId;
                account.IsActive = input.IsActive;
                if (userId == 0)
                {
                    account.PasswordHash = PasswordHasher.Hash(input.InitialPassword);
                    db.Users.Add(account);
                }
                db.SaveChanges();
                transaction.Commit();
            }
            TempData["Success"] = "Đã lưu tài khoản.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult ResetPassword(int id)
        {
            var account = db.Users.Find(id);
            if (account == null)
                return HttpNotFound();
            ViewBag.Username = account.Username;
            return View(new NewPasswordInput());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(int id, NewPasswordInput input)
        {
            var account = db.Users.Find(id);
            if (account == null)
                return HttpNotFound();
            ViewBag.Username = account.Username;
            if (!ModelState.IsValid)
                return View(input);
            account.PasswordHash = PasswordHasher.Hash(input.NewPassword);
            db.SaveChanges();
            TempData["Success"] = "Đã đặt lại mật khẩu.";
            return RedirectToAction("Index");
        }

        private ActionResult EditView(UserAdminInput input)
        {
            ViewBag.Roles = new SelectList(db.Roles.OrderBy(r => r.RoleCode).ToList(), "RoleId", "RoleCode", input.RoleId);
            return View("Edit", input);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
