using System.Linq;
using System.Web.Http;
using MedicalEquipmentManagement.Data;
namespace MedicalEquipmentManagement.Api
{
    [RoutePrefix("api/equipments")]
    public class EquipmentApiController : ApiController
    {
        private readonly MedicalEquipmentContext db = new MedicalEquipmentContext();
        [HttpGet, Route("")]
        public IHttpActionResult GetAll() { return Ok(db.Equipments.ToList()); }
        [HttpGet, Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            var item = db.Equipments.Find(id);
            if (item == null) return NotFound();
            return Ok(item);
        }
        // TODO Member 4: POST / PUT / DELETE + validation + DTO.
    }
}
