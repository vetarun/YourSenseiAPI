using System.Threading.Tasks;
using System.Web.Http;
using YourSensei.Service;
using YourSensei.ViewModel;

namespace YourSensei.Controllers
{
   [Authorize]
    [RoutePrefix("Mentor")]
    public class MentorController : ApiController
    {
        private readonly IMentorService _service;

        public MentorController(MentorService service)
        {
            _service = service;
        }


        [HttpGet]
        [Route("GetAllMentors")]
        public async Task<IHttpActionResult> GetAllMentorsByCompanyID(string companyID)
        {
            var result = await _service.GetAllMentorsByCompanyID(companyID);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetAllMentorsAnonymous")]
        public async Task<IHttpActionResult> GetAllMentorsByCompanyIDAnonymous(string companyID)
        {
            var result = await _service.GetAllMentorsByCompanyID(companyID);
            return Ok(result);
        }

        [HttpPost]
        [Route("AddMentor")]
        public async Task<IHttpActionResult> AddMentor(MentorResponseViewModel emp)
        {
            var result = await _service.AddMentor(emp);
            return Ok(result);
        }

        [HttpGet]
        [Route("DeleteMentor")]
        public async Task<IHttpActionResult> DeleteMentor(string id)
        {
            var result = await _service.DeleteMentor(id);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetMentorById")]
        public async Task<IHttpActionResult> GetMentorById(string id)
        {
            if (id != null)
            {
                var result = await _service.GetMentorById(id);
                return Ok(result);
            }
            return Ok();
        }

        [HttpPost]
        [Route("UpdateMentor")]
        public async Task<IHttpActionResult> UpdateMentor(MentorResponseViewModel obj)
        {
            var result =await _service.UpdateMentor(obj);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetMentorsByIsActive")]
        public async Task<IHttpActionResult> GetMentorsByIsActive(string companyID, bool isActive)
        {
            var result = await _service.GetMentorsByIsActive(companyID, isActive);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetMentorByEmployeeID")]
        public async Task<IHttpActionResult> GetMentorByEmployeeID(string employeeID)
        {
            var result = await _service.GetMentorByEmployeeID(employeeID);
            return Ok(result);
        }
    }
}
