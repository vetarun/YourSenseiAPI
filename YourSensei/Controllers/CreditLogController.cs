using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using YourSensei.Service;

namespace YourSensei.Controllers
{
    [Authorize]
    [RoutePrefix("CreditLog")]
    public class CreditLogController : ApiController
    {
        private readonly ICreditLogService _service;

        public CreditLogController(CreditLogService service)
        {
            _service = service;
        }

        [HttpGet]
        [Route("GetCreditLogsByCompanyID")]
        public async Task<IHttpActionResult> GetCreditLogsByCompanyID(string companyID)
        {
            var result = await _service.GetCreditLogsByCompanyID(companyID);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetAllEmployeewithMentor")]
        public async Task<IHttpActionResult> GetAllEmployeewithMentor(string userid)
        {
            var result = await _service.GetAllEmployeewithMentor(userid);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetCreditLogsByUserID")]
        public async Task<IHttpActionResult> GetCreditLogsByUserID(string userid, string companyID)
        {
            var result = await _service.GetCreditLogsByUserID(userid,companyID);
            return Ok(result);
        }

        [HttpGet]
        [Route("IsMentorLoggedIn")]
        public async Task<Boolean> IsMentorLoggedIn(string userid)
        {
            var result = await _service.IsMentorLoggedIn(userid);
            return result;
        }

        [HttpGet]
        [Route("GetCreditLogsByLoggedInUser")]
        public async Task<IHttpActionResult> GetCreditLogsByLoggedInUser(string userid, bool isActive)
        {
            var result = await _service.GetCreditLogsByLoggedInUser(userid, isActive);
            return Ok(result);
        }
    }
}