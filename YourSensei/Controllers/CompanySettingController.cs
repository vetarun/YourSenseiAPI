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
    [RoutePrefix("CompanySetting")]
    public class CompanySettingController : ApiController
    {
        private readonly ICompanySettingService _service;

        public CompanySettingController(CompanySettingService service)
        {
            _service = service;
        }

        [HttpPost]
        [Route("AddCompanySetting")]
        public async Task<IHttpActionResult> AddCompanySetting(CompanySettingViewModel obj)
        {
            var result = await _service.AddCompanySetting(obj);            
            return Ok(result);
        }
      
        [HttpGet]
        [Route("GetAllCompanySetting")]
        public async Task<IHttpActionResult> GetAllCompanySetting(string id)
        {
            var result = await _service.GetAllCompanySetting(id);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetCompanySettingbyId")]
        public async Task<IHttpActionResult> GetCompanySettingbyId(string id)
        {
            var result = await _service.GetCompanyById(id);
            return Ok(result);
        }
    }
}
