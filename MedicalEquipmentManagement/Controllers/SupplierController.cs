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
    /// Controller Quản lý Nhà cung cấp & Hãng sản xuất (Supplier) - CRUD cơ bản.
    /// </summary>
    public class SupplierController : Controller
    {
        private readonly MedicalEquipmentContext db = new MedicalEquipmentContext();

        // GET: Supplier
        public ActionResult Index(string keyword)
        {
            var query = db.Suppliers
                .Include(s => s.ManufacturedModels)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(s =>
                    s.Name.Contains(keyword) ||
                    (s.Phone != null && s.Phone.Contains(keyword)) ||
                    (s.Email != null && s.Email.Contains(keyword)));
            }

            return View(new KeywordSearchViewModel<Supplier>
            {
                Keyword = keyword,
                Items = query.OrderBy(s => s.Name).ToList()
            });
        }

        // GET: Supplier/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var supplier = db.Suppliers
                .Include(s => s.ManufacturedModels.Select(m => m.Category))
                .FirstOrDefault(s => s.SupplierId == id.Value);

            if (supplier == null)
            {
                return HttpNotFound();
            }

            return View(supplier);
        }

        // GET: Supplier/Create
        public ActionResult Create()
        {
            return View(new Supplier
            {
                SupplierType = SupplierTypeConst.Manufacturer,
                IsActive = true
            });
        }

        // POST: Supplier/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name,SupplierType,Phone,Email,IsActive")] Supplier supplier)
        {
            if (!string.IsNullOrWhiteSpace(supplier.Name))
            {
                supplier.Name = supplier.Name.Trim();
            }

            if (ModelState.IsValid)
            {
                bool nameExists = db.Suppliers.Any(s => s.Name.ToLower() == supplier.Name.ToLower());
                if (nameExists)
                {
                    ModelState.AddModelError("Name", "Tên nhà cung cấp / hãng sản xuất này đã tồn tại.");
                    return View(supplier);
                }

                db.Suppliers.Add(supplier);
                db.SaveChanges();
                TempData["Success"] = $"Đã thêm nhà cung cấp '{supplier.Name}' thành công.";
                return RedirectToAction("Index");
            }

            return View(supplier);
        }

        // GET: Supplier/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var supplier = db.Suppliers.Find(id);
            if (supplier == null)
            {
                return HttpNotFound();
            }

            return View(supplier);
        }

        // POST: Supplier/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "SupplierId,Name,SupplierType,Phone,Email,IsActive")] Supplier supplier)
        {
            if (!string.IsNullOrWhiteSpace(supplier.Name))
            {
                supplier.Name = supplier.Name.Trim();
            }

            if (ModelState.IsValid)
            {
                bool nameExists = db.Suppliers.Any(s =>
                    s.Name.ToLower() == supplier.Name.ToLower() &&
                    s.SupplierId != supplier.SupplierId);

                if (nameExists)
                {
                    ModelState.AddModelError("Name", "Tên nhà cung cấp này đã trùng với đơn vị khác.");
                    return View(supplier);
                }

                var existing = db.Suppliers.Find(supplier.SupplierId);
                if (existing == null)
                {
                    return HttpNotFound();
                }

                existing.Name = supplier.Name;
                existing.SupplierType = supplier.SupplierType;
                existing.Phone = supplier.Phone;
                existing.Email = supplier.Email;
                existing.IsActive = supplier.IsActive;

                db.SaveChanges();
                TempData["Success"] = $"Đã cập nhật nhà cung cấp '{existing.Name}' thành công.";
                return RedirectToAction("Index");
            }

            return View(supplier);
        }

        // GET: Supplier/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var supplier = db.Suppliers
                .Include(s => s.ManufacturedModels)
                .FirstOrDefault(s => s.SupplierId == id.Value);

            if (supplier == null)
            {
                return HttpNotFound();
            }

            return View(supplier);
        }

        // POST: Supplier/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var supplier = db.Suppliers
                .Include(s => s.ManufacturedModels)
                .FirstOrDefault(s => s.SupplierId == id);

            if (supplier == null)
            {
                return HttpNotFound();
            }

            int modelCount = supplier.ManufacturedModels != null ? supplier.ManufacturedModels.Count : 0;
            if (modelCount > 0)
            {
                TempData["Error"] = $"Không thể xóa nhà cung cấp '{supplier.Name}' vì đang có {modelCount} mẫu thiết bị thuộc hãng này.";
                return RedirectToAction("Index");
            }

            try
            {
                db.Suppliers.Remove(supplier);
                db.SaveChanges();
                TempData["Success"] = $"Đã xóa nhà cung cấp '{supplier.Name}' thành công.";
            }
            catch (DbUpdateException ex) when (DbErrorHelper.IsForeignKeyViolation(ex))
            {
                TempData["Error"] = $"Không thể xóa '{supplier.Name}' do đang được tham chiếu trong cơ sở dữ liệu.";
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
