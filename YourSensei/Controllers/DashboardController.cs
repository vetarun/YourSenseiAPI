using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using YourSensei.Service;
using YourSensei.ViewModel;

namespace YourSensei.Controllers
{
    [Authorize]
    [RoutePrefix("Dashboard")]
    public class DashboardController : ApiController
    {
        private readonly IDashboardService _service;
        public DashboardController(DashboardService service)
        {
            _service = service;
        }

        [HttpGet]
        [Route("GetBeltList")]
        public async Task<IHttpActionResult> GetBeltList(int id, string companyId, string UserId)
        {
            var result = await _service.GetBeltList(id, companyId, UserId);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("AddUpdateBelt")]
        public async Task<IHttpActionResult> AddUpdateBelt(BeltRuleInputViewModel belt)
        {
            var result = await _service.AddUpdateBelt(belt);
            return Ok(result);
        }

        [HttpGet]
        [Route("DeleteBelt")]
        public async Task<IHttpActionResult> DeleteBelt(int id, string userid)
        {
            var result = await _service.DeleteBelt(id,userid);
            return Ok(result);
        }
        [HttpPost]
        [Route("GetDashboardBeltDetails")]
        public async Task<IHttpActionResult> GetDashboardBeltDetails(string companyId, string UserId, string employeeID, bool mentor)
        {
            var result = await _service.GetDashboardBeltDetails(companyId,UserId,employeeID,mentor);
            return Ok(result);
        }


        [HttpGet]
        [Route("GetCreditStandings")]
        public async Task<IHttpActionResult> GetCreditStandings(string companyID, string userDetailID,bool isMentor)
        {
            var result = await _service.GetCreditStandings(companyID, userDetailID,isMentor);
            return Ok(result);
        }
    }
}
