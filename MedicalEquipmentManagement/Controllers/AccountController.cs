using MedicalEquipmentManagement.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
namespace MedicalEquipmentManagement.Controllers
{
    public class AccountController : Controller
    {
        private MedicalEquipmentContext db = new MedicalEquipmentContext();
        public static string GetMd5Hash(string input)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                return sBuilder.ToString();
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

            string hashedPassword = GetMd5Hash(password);

            // Kiểm tra tài khoản trong DB
            var user = db.Users.FirstOrDefault(u => u.Username == username
                                                && (u.PasswordHash == hashedPassword || u.PasswordHash == password)
                                                && u.IsActive);

            if (user != null)
            {
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
