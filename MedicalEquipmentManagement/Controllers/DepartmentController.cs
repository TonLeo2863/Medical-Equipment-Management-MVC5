using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using MedicalEquipmentManagement.Data;
using MedicalEquipmentManagement.Models;

namespace MedicalEquipmentManagement.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly MedicalEquipmentContext db = new MedicalEquipmentContext();

        // GET: Department
        public ActionResult Index(string keyword)
        {
            var query = db.Departments.Include(d => d.EquipmentAssets).AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(d => d.Name.Contains(keyword) || d.DepartmentCode.Contains(keyword) || d.Location.Contains(keyword));
            }

            var departments = query.OrderBy(d => d.DepartmentCode).ToList();
            ViewBag.Keyword = keyword;
            return View(departments);
        }

        // GET: Department/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var department = db.Departments
                .Include(d => d.EquipmentAssets.Select(e => e.EquipmentModel))
                .FirstOrDefault(d => d.DepartmentId == id.Value);

            if (department == null)
            {
                return HttpNotFound();
            }

            return View(department);
        }

        // GET: Department/Create
        public ActionResult Create()
        {
            var model = new Department { IsActive = true };
            return View(model);
        }

        // POST: Department/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "DepartmentCode,Name,Location,IsActive")] Department department)
        {
            if (ModelState.IsValid)
            {
                var codeExists = db.Departments.Any(d => d.DepartmentCode.Trim().ToLower() == department.DepartmentCode.Trim().ToLower());
                if (codeExists)
                {
                    ModelState.AddModelError("DepartmentCode", "Mã khoa/phòng này đã tồn tại trong hệ thống.");
                    return View(department);
                }

                db.Departments.Add(department);
                db.SaveChanges();
                TempData["Success"] = $"Đã thêm thành công khoa/phòng: {department.Name}";
                return RedirectToAction("Index");
            }

            return View(department);
        }

        // GET: Department/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var department = db.Departments.Find(id);
            if (department == null)
            {
                return HttpNotFound();
            }

            return View(department);
        }

        // POST: Department/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "DepartmentId,DepartmentCode,Name,Location,IsActive")] Department department)
        {
            if (ModelState.IsValid)
            {
                var codeExists = db.Departments.Any(d => d.DepartmentCode.Trim().ToLower() == department.DepartmentCode.Trim().ToLower() && d.DepartmentId != department.DepartmentId);
                if (codeExists)
                {
                    ModelState.AddModelError("DepartmentCode", "Mã khoa/phòng này đã thuộc phòng ban khác.");
                    return View(department);
                }

                db.Entry(department).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = $"Cập nhật thông tin khoa/phòng {department.Name} thành công.";
                return RedirectToAction("Index");
            }

            return View(department);
        }

        // GET: Department/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var department = db.Departments
                .Include(d => d.EquipmentAssets)
                .FirstOrDefault(d => d.DepartmentId == id.Value);

            if (department == null)
            {
                return HttpNotFound();
            }

            return View(department);
        }

        // POST: Department/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var department = db.Departments
                .Include(d => d.EquipmentAssets)
                .FirstOrDefault(d => d.DepartmentId == id);

            if (department == null)
            {
                return HttpNotFound();
            }

            // Kiểm tra ràng buộc: Không cho xóa nếu đang có thiết bị gắn với phòng ban này
            if (department.EquipmentAssets != null && department.EquipmentAssets.Any())
            {
                TempData["Error"] = $"Không thể xóa khoa/phòng '{department.Name}' vì hiện đang có {department.EquipmentAssets.Count} thiết bị y tế trực thuộc.";
                return RedirectToAction("Index");
            }

            db.Departments.Remove(department);
            db.SaveChanges();
            TempData["Success"] = $"Đã xóa khoa/phòng {department.Name} thành công.";
            return RedirectToAction("Index");
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
