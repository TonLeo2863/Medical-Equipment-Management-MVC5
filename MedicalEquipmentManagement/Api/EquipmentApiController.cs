using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using MedicalEquipmentManagement.Data;
using MedicalEquipmentManagement.Helpers;
using MedicalEquipmentManagement.Models;
using MedicalEquipmentManagement.ViewModels;

namespace MedicalEquipmentManagement.Api
{
    /// <summary>Cung cấp CRUD thiết bị dưới dạng JSON và kiểm tra Session, vai trò và token trên các phương thức ghi.</summary>
    [RoutePrefix("api/equipments")]
    public class EquipmentApiController : ApiController
    {
        private readonly MedicalEquipmentContext db = new MedicalEquipmentContext();

        [HttpGet, Route("")]
        public IHttpActionResult GetAll()
        {
            var denied = CheckAccess(false);
            if (denied != null)
                return denied;
            return Ok(db.EquipmentAssets.Where(a => !a.IsDeleted).OrderBy(a => a.AssetId)
                .ToList().Select(EquipmentAssetViewModel.FromEntity).ToList());
        }

        [HttpGet, Route("{id:int}", Name = "GetEquipmentAsset")]
        public IHttpActionResult GetById(int id)
        {
            var denied = CheckAccess(false);
            if (denied != null)
                return denied;
            var asset = FindAsset(id);
            if (asset == null)
                return NotFound();
            return Ok(EquipmentAssetViewModel.FromEntity(asset));
        }

        [HttpPost, Route("")]
        public IHttpActionResult Create(EquipmentAssetViewModel input)
        {
            var denied = CheckAccess(true);
            if (denied != null)
                return denied;
            var asset = new EquipmentAsset();
            var error = SaveAsset(input, asset, true);
            if (error != null)
                return error;
            return CreatedAtRoute("GetEquipmentAsset", new { id = asset.AssetId }, EquipmentAssetViewModel.FromEntity(asset));
        }

        [HttpPut, Route("{id:int}")]
        public IHttpActionResult Update(int id, EquipmentAssetViewModel input)
        {
            var denied = CheckAccess(true);
            if (denied != null)
                return denied;
            using (var transaction = db.Database.BeginTransaction())
            {
                db.LockAsset(id);
                var asset = FindAsset(id);
                if (asset == null)
                    return NotFound();
                var error = SaveAsset(input, asset, false);
                if (error != null)
                    return error;
                transaction.Commit();
                return StatusCode(HttpStatusCode.NoContent);
            }
        }

        [HttpDelete, Route("{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            var denied = CheckAccess(true);
            if (denied != null)
                return denied;
            using (var transaction = db.Database.BeginTransaction())
            {
                db.LockAsset(id);
                var asset = FindAsset(id);
                if (asset == null)
                    return NotFound();
                if (EquipmentPolicy.RemovalConflict(db, id) != null)
                    return Conflict();
                asset.IsDeleted = true;
                db.SaveChanges();
                transaction.Commit();
                return StatusCode(HttpStatusCode.NoContent);
            }
        }

        private EquipmentAsset FindAsset(int id)
        {
            return db.EquipmentAssets.FirstOrDefault(a => a.AssetId == id && !a.IsDeleted);
        }

        /// <summary>Áp dụng validation chung với MVC. Xung đột mã định danh hoặc ràng buộc đồng thời trả HTTP 409.</summary>
        private IHttpActionResult SaveAsset(EquipmentAssetViewModel input, EquipmentAsset asset, bool isNew)
        {
            if (input == null || !ModelState.IsValid)
                return BadRequest(ModelState);
            var fieldError = EquipmentPolicy.ValidateFields(db, input, isNew ? null : asset);
            if (fieldError != null)
                return BadRequest(fieldError);
            if (!isNew && EquipmentPolicy.ValidateChange(db, input, asset) != null)
                return Conflict();

            input.ApplyToEntity(asset);
            if (EquipmentPolicy.HasDuplicate(db, asset))
                return Conflict();
            if (isNew)
                db.EquipmentAssets.Add(asset);
            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException error) when (error.GetBaseException() is System.Data.SqlClient.SqlException sql
                && (sql.Number == 2601 || sql.Number == 2627 || sql.Number == 547))
            {
                return Conflict();
            }
            return null;
        }

        /// <summary>Trả HTTP 401 cho phiên không hợp lệ, 403 khi sai quyền và 400 khi yêu cầu ghi thiếu token chống CSRF.</summary>
        private IHttpActionResult CheckAccess(bool write)
        {
            var context = HttpContext.Current;
            var session = context?.Session;
            var role = SessionAccount.GetRole(session == null ? null : new HttpSessionStateWrapper(session), db);
            if (role == null)
                return StatusCode(HttpStatusCode.Unauthorized);

            if (role != "ADMIN" && role != "MANAGER"
                && (write || (role != "STAFF" && role != "ENGINEER")))
                return StatusCode(HttpStatusCode.Forbidden);
            if (!write)
                return null;

            try
            {
                AntiForgery.Validate(context.Request.Cookies[AntiForgeryConfig.CookieName]?.Value,
                    context.Request.Headers["RequestVerificationToken"]);
                return null;
            }
            catch (System.Web.Mvc.HttpAntiForgeryException)
            {
                return BadRequest("Token xác minh yêu cầu không hợp lệ.");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
