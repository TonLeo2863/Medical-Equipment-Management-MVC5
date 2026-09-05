using System.Web.Mvc;
namespace MedicalEquipmentManagement.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Login() { return View(); }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            // TODO Member 1: authenticate against Users, store user in Session.
            if (username == "admin" && password == "admin")
            {
                Session["UserName"] = username;
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Message = "Tên đăng nhập hoặc mật khẩu không đúng.";
            return View();
        }
        public ActionResult Logout() { Session.Clear(); return RedirectToAction("Login"); }
    }
}
