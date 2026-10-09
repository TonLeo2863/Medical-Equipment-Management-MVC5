using System.Linq;
using System.Web.Mvc;
using MedicalEquipmentManagement.Data;
namespace MedicalEquipmentManagement.Controllers
{
    public class EquipmentController : Controller
    {
        private readonly MedicalEquipmentContext db = new MedicalEquipmentContext();
        public ActionResult Index(string keyword, int? categoryId)
        {
            var q = db.EquipmentModels.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                q = q.Where(x => x.ModelName.Contains(keyword));
            }

            if (categoryId.HasValue)
            {
                q = q.Where(x => x.CategoryId == categoryId.Value);
            }

            return View(q.OrderBy(x => x.ModelName).ToList());
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
