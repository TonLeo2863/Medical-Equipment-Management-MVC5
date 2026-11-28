using System;
using System.Data.Entity;
using System.Diagnostics;
using MedicalEquipmentManagement.Data;
using MedicalEquipmentManagement.Helpers;
using System.Linq;
using System.Web.Mvc;
using MedicalEquipmentManagement.Filters;
using MedicalEquipmentManagement.ViewModels;
namespace MedicalEquipmentManagement.Controllers
{
    /// <summary>Xác thực người dùng, quản lý Session và thay đổi mật khẩu cá nhân.</summary>
    public class AccountController : Controller
    {
        private MedicalEquipmentContext db = new MedicalEquipmentContext();
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        /// <summary>Xác minh tài khoản hoạt động rồi tạo Session với vai trò lấy từ SQL Server.</summary>
        /// <param name="input">Tên đăng nhập và mật khẩu từ biểu mẫu có token chống CSRF.</param>
        /// <returns>Chuyển đến Dashboard khi xác thực thành công hoặc trả lại biểu mẫu với lỗi chung.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginInput input)
        {
            if (!ModelState.IsValid)
                return View(input);

            var timer = Stopwatch.StartNew();
            db.Database.Initialize(false);
            var initialization = timer.ElapsedMilliseconds;
            var user = db.Users.AsNoTracking().Include(u => u.Role)
                .SingleOrDefault(u => u.Username == input.Username.Trim() && u.IsActive);
            var query = timer.ElapsedMilliseconds - initialization;
            var verified = user != null && user.Role != null && PasswordHasher.Verify(input.Password, user.PasswordHash);
            var derivation = timer.ElapsedMilliseconds - initialization - query;
            Trace.TraceInformation("Login EF init={0} ms, EF query={1} ms, PBKDF2={2} ms",
                initialization, query, derivation);
            if (Request.IsLocal && Environment.GetEnvironmentVariable("MEDICAL_QA_TIMINGS") == "1")
                Response.AppendHeader("Server-Timing", $"ef_init;dur={initialization}, ef_query;dur={query}, pbkdf2;dur={derivation}");
            if (verified)
            {
                Session.Clear();
                Session["UserId"] = user.UserId;
                Session["UserName"] = user.Username;
                Session["FullName"] = user.FullName;
                Session["RoleCode"] = user.Role.RoleCode;
                Session["Credential"] = PasswordHasher.SessionCredential(user.PasswordHash);
                SessionAccount.BindLogin(HttpContext);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
            return View(input);
        }

        /// <summary>Hủy Session bằng POST có token để trang bên ngoài không thể chủ động đăng xuất người dùng.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            SessionAccount.ExpireLogin(HttpContext);
            return RedirectToAction("Login", "Account");
        }

        [HttpGet, ActionName("Logout")]
        public ActionResult LogoutRequiresPost()
        {
            Response.AppendHeader("Allow", "POST");
            return new HttpStatusCodeResult(405);
        }

        [HttpGet]
        [CustomAuthorize]
        public ActionResult ChangePassword()
        {
            return View(new ChangePasswordInput());
        }

        /// <summary>Xác minh mật khẩu cũ và cập nhật có điều kiện để tránh ghi đè lần đặt lại xảy ra đồng thời.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize]
        public ActionResult ChangePassword(ChangePasswordInput input)
        {
            if (!ModelState.IsValid)
                return View(input);
            var account = db.Users.Find((int)Session["UserId"]);
            if (account == null)
                return HttpNotFound();
            var previousHash = account.PasswordHash;
            if (!PasswordHasher.Verify(input.CurrentPassword, previousHash))
            {
                ModelState.AddModelError(nameof(input.CurrentPassword), "Mật khẩu hiện tại không đúng.");
                return View(input);
            }

            var newHash = PasswordHasher.Hash(input.NewPassword);
            var rows = db.Database.ExecuteSqlCommand(
                "UPDATE dbo.Users SET PasswordHash = @p0 WHERE UserId = @p1 AND PasswordHash = @p2 AND IsActive = 1",
                newHash, account.UserId, previousHash);
            if (rows != 1)
            {
                ModelState.AddModelError("", "Tài khoản hoặc mật khẩu vừa thay đổi. Vui lòng đăng nhập lại rồi thử lại.");
                return View(input);
            }
            Session["Credential"] = PasswordHasher.SessionCredential(newHash);
            TempData["Success"] = "Đã đổi mật khẩu.";
            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
