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
    [RoutePrefix("Employee")]
    public class EmployeeController : ApiController
    {
        private readonly IEmployeeService _service;

        public EmployeeController(EmployeeService service)
        {
            _service = service;
        }

        [HttpGet]
        [Route("GetAllEmployee")]
        public async Task<IHttpActionResult> GetAllEmployee(string companyid)
        {
            var result = await _service.GetAllEmployee( companyid);
            return Ok(result);
        }

        [HttpPost]
        [Route("AddEmployee")]
        public async Task<IHttpActionResult> AddEmployee(EmployeeResponseViewModel emp)
        {
            var result = await _service.AddEmployee(emp);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetAllRole")]
        public async Task<IHttpActionResult> GetAllRole()
        {
            var result = await _service.GetAllRole();
            return Ok(result);
        }

        [HttpGet]
        [Route("GetAllMentor")]
        public async Task<IHttpActionResult> GetAllMentor(string companyid)
        {
            var result = await _service.GetAllMentor(companyid);
            return Ok(result);
        }

        [HttpGet]
        [Route("DeleteEmployee")]
        public async Task<IHttpActionResult> DeleteEmployee(string id)
        {
            var result = await _service.DeleteEmployee(id);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetEmployeeById")]
        public async Task<IHttpActionResult> GetEmployeeById(string id)
        {
            if (id != null)
            {
                var result = await _service.GetEmployeeById(id);
                return Ok(result);
            }
            return Ok();
        }

        [HttpPost]
        [Route("UpdateEmployee")]
        public async Task<IHttpActionResult> UpdateEmployee(EmployeeResponseViewModel obj)
        {
            var result =await _service.UpdateEmployee(obj);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetProfileByEmail")]
        public async Task<IHttpActionResult> GetProfileByEmail(string email)
        {
            var result = await _service.GetProfileByEmail(email);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetMentorProfileByEmail")]
        public async Task<IHttpActionResult> GetMentorProfileByEmail(string email)
        {
            var result = await _service.GetMentorProfileByEmail(email);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetEmployeeByMentorID")]
        public async Task<IHttpActionResult> GetEmployeeByMentorID(string mentorID)
        {
            var result = await _service.GetEmployeeByMentorID(mentorID);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetEmployeeByUserDetailID")]
        public async Task<IHttpActionResult> GetEmployeeByUserDetailID(Guid userID)
        {
            var result = await _service.GetEmployeeByUserDetailID(userID);
            return Ok(result);
        }
    }
}
