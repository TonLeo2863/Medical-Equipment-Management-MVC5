using System;
using MedicalEquipmentManagement.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Web.Mvc;
namespace MedicalEquipmentManagement.Controllers
{
    public class AccountController : Controller
    {
        private MedicalEquipmentContext db = new MedicalEquipmentContext();
        private static bool VerifyPassword(string password, string storedHash)
        {
            var fields = storedHash?.Split('$');
            if (fields == null || fields.Length != 3 || fields[0] != "pbkdf2-sha256-v1")
                return false;

            try
            {
                var salt = Convert.FromBase64String(fields[1]);
                var expected = Convert.FromBase64String(fields[2]);
                if (salt.Length != 16 || expected.Length != 32)
                    return false;

                using (var derivation = new Rfc2898DeriveBytes(password, salt, 600000, HashAlgorithmName.SHA256))
                {
                    var actual = derivation.GetBytes(expected.Length);
                    int difference = 0;
                    for (int i = 0; i < expected.Length; i++)
                        difference |= expected[i] ^ actual[i];
                    return difference == 0;
                }
            }
            catch (FormatException)
            {
                return false;
            }
        }
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Message = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.";
                return View();
            }

            var user = db.Users.FirstOrDefault(u => u.Username == username && u.IsActive);
            if (user != null && VerifyPassword(password, user.PasswordHash))
            {
                Session["UserId"] = user.UserId;
                Session["UserName"] = user.Username;
                Session["FullName"] = user.FullName;
                Session["RoleCode"] = user.Role != null ? user.Role.RoleCode : "STAFF";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Message = "Tên đăng nhập hoặc mật khẩu không đúng.";
            return View();
        }

        // Get: Account/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login", "Account");
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
