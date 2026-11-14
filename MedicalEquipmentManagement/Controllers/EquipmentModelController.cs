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
    /// Controller Quản lý Mẫu thiết bị (EquipmentModel):
    /// CRUD thông tin mẫu máy, hãng sản xuất, có Dropdown chọn Category (hiển thị đúng tên).
    /// </summary>
    public class EquipmentModelController : Controller
    {
        private readonly MedicalEquipmentContext db = new MedicalEquipmentContext();

        // GET: EquipmentModel
        public ActionResult Index(string keyword, int? categoryId)
        {
            var query = db.EquipmentModels
                .Include(m => m.Category)
                .Include(m => m.ManufacturerSupplier)
                .Include(m => m.EquipmentAssets)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(m =>
                    m.ModelName.Contains(keyword) ||
                    m.ModelCode.Contains(keyword) ||
                    (m.ManufacturerSupplier != null && m.ManufacturerSupplier.Name.Contains(keyword)));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(m => m.CategoryId == categoryId.Value);
            }

            var vm = new EquipmentModelIndexViewModel
            {
                Keyword = keyword,
                CategoryId = categoryId,
                Items = query.OrderBy(m => m.ModelCode).ToList(),
                CategoryList = db.EquipmentCategories
                    .OrderBy(c => c.Name)
                    .ToList()
                    .Select(c => new SelectListItem
                    {
                        Value = c.CategoryId.ToString(),
                        Text = c.Name,
                        Selected = categoryId.HasValue && categoryId.Value == c.CategoryId
                    })
                    .ToList()
            };

            return View(vm);
        }

        // GET: EquipmentModel/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = db.EquipmentModels
                .Include(m => m.Category)
                .Include(m => m.ManufacturerSupplier)
                .Include(m => m.EquipmentAssets.Select(a => a.Department))
                .FirstOrDefault(m => m.ModelId == id.Value);

            if (model == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        // GET: EquipmentModel/Create
        public ActionResult Create(int? categoryId)
        {
            var vm = new EquipmentModelFormViewModel
            {
                CategoryId = categoryId,
                IsActive = true
            };
            PopulateDropdownLists(vm);
            return View(vm);
        }

        // POST: EquipmentModel/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EquipmentModelFormViewModel vm)
        {
            if (!string.IsNullOrWhiteSpace(vm.ModelCode))
            {
                vm.ModelCode = vm.ModelCode.Trim().ToUpperInvariant();
            }

            if (ModelState.IsValid)
            {
                bool codeExists = db.EquipmentModels.Any(m =>
                    m.ModelCode.ToLower() == vm.ModelCode.ToLower());

                if (codeExists)
                {
                    ModelState.AddModelError("ModelCode", "Mã mẫu thiết bị (Model Code) này đã tồn tại.");
                }
                else
                {
                    var entity = new EquipmentModel();
                    vm.ApplyToEntity(entity);

                    db.EquipmentModels.Add(entity);
                    db.SaveChanges();

                    TempData["Success"] = $"Đã thêm mẫu thiết bị [{entity.ModelCode}] {entity.ModelName} thành công.";
                    return RedirectToAction("Index");
                }
            }

            PopulateDropdownLists(vm);
            return View(vm);
        }

        // GET: EquipmentModel/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var entity = db.EquipmentModels.Find(id);
            if (entity == null)
            {
                return HttpNotFound();
            }

            var vm = EquipmentModelFormViewModel.FromEntity(entity);
            PopulateDropdownLists(vm);
            return View(vm);
        }

        // POST: EquipmentModel/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EquipmentModelFormViewModel vm)
        {
            if (!string.IsNullOrWhiteSpace(vm.ModelCode))
            {
                vm.ModelCode = vm.ModelCode.Trim().ToUpperInvariant();
            }

            if (ModelState.IsValid)
            {
                bool codeExists = db.EquipmentModels.Any(m =>
                    m.ModelCode.ToLower() == vm.ModelCode.ToLower() &&
                    m.ModelId != vm.ModelId);

                if (codeExists)
                {
                    ModelState.AddModelError("ModelCode", "Mã mẫu thiết bị này đã thuộc về một mẫu máy khác.");
                }
                else
                {
                    var entity = db.EquipmentModels.Find(vm.ModelId);
                    if (entity == null)
                    {
                        return HttpNotFound();
                    }

                    vm.ApplyToEntity(entity);
                    db.SaveChanges();

                    TempData["Success"] = $"Đã cập nhật mẫu thiết bị [{entity.ModelCode}] {entity.ModelName} thành công.";
                    return RedirectToAction("Index");
                }
            }

            PopulateDropdownLists(vm);
            return View(vm);
        }

        // GET: EquipmentModel/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var entity = db.EquipmentModels
                .Include(m => m.Category)
                .Include(m => m.ManufacturerSupplier)
                .Include(m => m.EquipmentAssets)
                .FirstOrDefault(m => m.ModelId == id.Value);

            if (entity == null)
            {
                return HttpNotFound();
            }

            return View(entity);
        }

        // POST: EquipmentModel/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var entity = db.EquipmentModels
                .Include(m => m.EquipmentAssets)
                .FirstOrDefault(m => m.ModelId == id);

            if (entity == null)
            {
                return HttpNotFound();
            }

            int assetCount = entity.EquipmentAssets != null ? entity.EquipmentAssets.Count : 0;
            if (assetCount > 0)
            {
                TempData["Error"] = $"Không thể xóa mẫu thiết bị '{entity.ModelName}' vì đang có {assetCount} tài sản thiết bị thực tế thuộc mẫu này.";
                return RedirectToAction("Index");
            }

            try
            {
                db.EquipmentModels.Remove(entity);
                db.SaveChanges();
                TempData["Success"] = $"Đã xóa mẫu thiết bị [{entity.ModelCode}] {entity.ModelName} thành công.";
            }
            catch (DbUpdateException ex) when (DbErrorHelper.IsForeignKeyViolation(ex))
            {
                TempData["Error"] = $"Không thể xóa mẫu thiết bị '{entity.ModelName}' do đang được tham chiếu trong cơ sở dữ liệu.";
            }

            return RedirectToAction("Index");
        }

        private void PopulateDropdownLists(EquipmentModelFormViewModel vm)
        {
            vm.CategoryList = db.EquipmentCategories
                .Where(c => c.IsActive || c.CategoryId == vm.CategoryId)
                .OrderBy(c => c.Name)
                .ToList()
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = $"{c.Name} ({c.CategoryCode})",
                    Selected = vm.CategoryId.HasValue && vm.CategoryId.Value == c.CategoryId
                })
                .ToList();

            vm.ManufacturerList = db.Suppliers
                .Where(s => s.IsActive || s.SupplierId == vm.ManufacturerId)
                .OrderBy(s => s.Name)
                .ToList()
                .Select(s => new SelectListItem
                {
                    Value = s.SupplierId.ToString(),
                    Text = s.Name,
                    Selected = vm.ManufacturerId.HasValue && vm.ManufacturerId.Value == s.SupplierId
                })
                .ToList();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
