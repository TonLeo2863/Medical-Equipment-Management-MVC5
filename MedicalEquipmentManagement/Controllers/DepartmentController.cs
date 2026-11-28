using MedicalEquipmentManagement.Filters;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using MedicalEquipmentManagement.Data;
using MedicalEquipmentManagement.Models;

namespace MedicalEquipmentManagement.Controllers
{
    [CustomAuthorize(Roles = "ADMIN")]
    public class DepartmentController : Controller
    {
        private readonly MedicalEquipmentContext db = new MedicalEquipmentContext();

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

        public ActionResult Create()
        {
            var model = new Department { IsActive = true };
            return View(model);
        }

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

                var stored = db.Departments.Find(department.DepartmentId);
                if (stored == null)
                    return HttpNotFound();
                stored.DepartmentCode = department.DepartmentCode.Trim();
                stored.Name = department.Name.Trim();
                stored.Location = department.Location?.Trim();
                stored.IsActive = department.IsActive;
                db.SaveChanges();
                TempData["Success"] = $"Cập nhật thông tin khoa/phòng {department.Name} thành công.";
                return RedirectToAction("Index");
            }

            return View(department);
        }

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

            if (db.AssetTransfers.Any(t => t.FromDepartmentId == id || t.ToDepartmentId == id)
                || db.InventoryTransactions.Any(t => t.DepartmentId == id))
            {
                TempData["Error"] = "Khoa/phòng có lịch sử kho hoặc điều chuyển. Có thể vô hiệu hóa từ trang Sửa.";
                return RedirectToAction("Index");
            }
            db.Departments.Remove(department);
            try
            {
                db.SaveChanges();
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException ex) when (MedicalEquipmentManagement.Helpers.DbErrorHelper.IsForeignKeyViolation(ex))
            {
                TempData["Error"] = "Khoa/phòng vừa phát sinh dữ liệu liên quan. Vui lòng kiểm tra lại.";
                return RedirectToAction("Index");
            }
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
