using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using MedicalEquipmentManagement.Data;
using MedicalEquipmentManagement.Models;
using MedicalEquipmentManagement.ViewModels;

namespace MedicalEquipmentManagement.Api
{
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
            var asset = FindAsset(id);
            if (asset == null)
                return NotFound();
            return SaveAsset(input, asset, false) ?? StatusCode(HttpStatusCode.NoContent);
        }

        [HttpDelete, Route("{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            var denied = CheckAccess(true);
            if (denied != null)
                return denied;
            var asset = FindAsset(id);
            if (asset == null)
                return NotFound();
            if (db.MaintenanceRecords.Any(m => m.AssetId == id))
                return Conflict();

            asset.IsDeleted = true;
            db.SaveChanges();
            return StatusCode(HttpStatusCode.NoContent);
        }

        private EquipmentAsset FindAsset(int id)
        {
            return db.EquipmentAssets.FirstOrDefault(a => a.AssetId == id && !a.IsDeleted);
        }

        private IHttpActionResult SaveAsset(EquipmentAssetViewModel input, EquipmentAsset asset, bool isNew)
        {
            if (input == null || !ModelState.IsValid)
                return BadRequest(ModelState);
            if (string.IsNullOrWhiteSpace(input.AssetCode) || string.IsNullOrWhiteSpace(input.SerialNumber)
                || !AssetStatus.All.Contains(input.Status)
                || !db.EquipmentModels.Any(m => m.ModelId == input.ModelId && m.IsActive)
                || (input.DepartmentId.HasValue && !db.Departments.Any(d => d.DepartmentId == input.DepartmentId && d.IsActive)))
                return BadRequest("Invalid asset fields or references.");

            input.ApplyToEntity(asset);
            if (db.EquipmentAssets.Any(a => a.AssetId != asset.AssetId
                && (a.AssetCode == asset.AssetCode || a.SerialNumber == asset.SerialNumber)))
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

        private IHttpActionResult CheckAccess(bool write)
        {
            var context = HttpContext.Current;
            var session = context?.Session;
            if (!(session?["UserId"] is int))
                return StatusCode(HttpStatusCode.Unauthorized);

            var role = session["RoleCode"] as string;
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
                return BadRequest("Invalid request verification token.");
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
