using System.Web.Mvc;
namespace MedicalEquipmentManagement.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index() { return View(); }
        public ActionResult About() { ViewBag.Message = "Website quản lý trang thiết bị y tế"; return View(); }
    }
}
