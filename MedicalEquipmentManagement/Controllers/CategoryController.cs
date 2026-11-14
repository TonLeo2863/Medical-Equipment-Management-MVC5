using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using MedicalEquipmentManagement.Data;
using MedicalEquipmentManagement.Helpers;
using MedicalEquipmentManagement.Models;
using MedicalEquipmentManagement.ViewModels;

namespace MedicalEquipmentManagement.Controllers
{
    /// <summary>
    /// Controller Quản lý Danh mục loại thiết bị y tế (Category) - CRUD cơ bản.
    /// </summary>
    public class CategoryController : Controller
    {
        private readonly MedicalEquipmentContext db = new MedicalEquipmentContext();

        // GET: Category
        public ActionResult Index(string keyword)
        {
            var query = db.EquipmentCategories
                .Include(c => c.EquipmentModels)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(c => c.Name.Contains(keyword) || c.CategoryCode.Contains(keyword));
            }

            return View(new KeywordSearchViewModel<EquipmentCategory>
            {
                Keyword = keyword,
                Items = query.OrderBy(c => c.CategoryCode).ToList()
            });
        }

        // GET: Category/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var category = db.EquipmentCategories
                .Include(c => c.EquipmentModels.Select(m => m.ManufacturerSupplier))
                .FirstOrDefault(c => c.CategoryId == id.Value);

            if (category == null)
            {
                return HttpNotFound();
            }

            return View(category);
        }

        // GET: Category/Create
        public ActionResult Create()
        {
            return View(new EquipmentCategory { IsActive = true });
        }

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CategoryCode,Name,IsActive")] EquipmentCategory category)
        {
            if (!string.IsNullOrWhiteSpace(category.CategoryCode))
            {
                category.CategoryCode = category.CategoryCode.Trim().ToUpperInvariant();
            }
            if (!string.IsNullOrWhiteSpace(category.Name))
            {
                category.Name = category.Name.Trim();
            }

            if (ModelState.IsValid)
            {
                bool codeExists = db.EquipmentCategories.Any(c =>
                    c.CategoryCode.ToLower() == category.CategoryCode.ToLower());

                if (codeExists)
                {
                    ModelState.AddModelError("CategoryCode", "Mã danh mục này đã tồn tại trong hệ thống.");
                    return View(category);
                }

                db.EquipmentCategories.Add(category);
                db.SaveChanges();
                TempData["Success"] = $"Đã thêm danh mục [{category.CategoryCode}] {category.Name} thành công.";
                return RedirectToAction("Index");
            }

            return View(category);
        }

        // GET: Category/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var category = db.EquipmentCategories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }

            return View(category);
        }

        // POST: Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CategoryId,CategoryCode,Name,IsActive")] EquipmentCategory category)
        {
            if (!string.IsNullOrWhiteSpace(category.CategoryCode))
            {
                category.CategoryCode = category.CategoryCode.Trim().ToUpperInvariant();
            }
            if (!string.IsNullOrWhiteSpace(category.Name))
            {
                category.Name = category.Name.Trim();
            }

            if (ModelState.IsValid)
            {
                bool codeExists = db.EquipmentCategories.Any(c =>
                    c.CategoryCode.ToLower() == category.CategoryCode.ToLower() &&
                    c.CategoryId != category.CategoryId);

                if (codeExists)
                {
                    ModelState.AddModelError("CategoryCode", "Mã danh mục này đã thuộc về một danh mục khác.");
                    return View(category);
                }

                var existing = db.EquipmentCategories.Find(category.CategoryId);
                if (existing == null)
                {
                    return HttpNotFound();
                }

                existing.CategoryCode = category.CategoryCode;
                existing.Name = category.Name;
                existing.IsActive = category.IsActive;

                db.SaveChanges();
                TempData["Success"] = $"Đã cập nhật danh mục [{existing.CategoryCode}] {existing.Name} thành công.";
                return RedirectToAction("Index");
            }

            return View(category);
        }

        // GET: Category/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var category = db.EquipmentCategories
                .Include(c => c.EquipmentModels)
                .FirstOrDefault(c => c.CategoryId == id.Value);

            if (category == null)
            {
                return HttpNotFound();
            }

            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var category = db.EquipmentCategories
                .Include(c => c.EquipmentModels)
                .FirstOrDefault(c => c.CategoryId == id);

            if (category == null)
            {
                return HttpNotFound();
            }

            int modelCount = category.EquipmentModels != null ? category.EquipmentModels.Count : 0;
            if (modelCount > 0)
            {
                TempData["Error"] = $"Không thể xóa danh mục '{category.Name}' vì đang có {modelCount} mẫu thiết bị thuộc danh mục này.";
                return RedirectToAction("Index");
            }

            try
            {
                db.EquipmentCategories.Remove(category);
                db.SaveChanges();
                TempData["Success"] = $"Đã xóa danh mục [{category.CategoryCode}] {category.Name} thành công.";
            }
            catch (DbUpdateException ex) when (DbErrorHelper.IsForeignKeyViolation(ex))
            {
                TempData["Error"] = $"Không thể xóa danh mục '{category.Name}' do dữ liệu đang được tham chiếu.";
            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
