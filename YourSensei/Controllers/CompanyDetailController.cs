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
    [RoutePrefix("CompanyDetail")]
    public class CompanyDetailController : ApiController
    {
        private readonly ICompanyDetailService _service;

        public CompanyDetailController(CompanyDetailService service)
        {
            _service = service;
        }

        [HttpGet]
        [Route("GetProfileByID")]
        public async Task<IHttpActionResult> GetProfileByID(string id)
        {
            var result = await _service.GetProfileByID(id);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetCompanies")]
        public async Task<IHttpActionResult> GetCompanies()
        {
            var result = await _service.GetCompanies();
            return Ok(result);
        }
    }
}