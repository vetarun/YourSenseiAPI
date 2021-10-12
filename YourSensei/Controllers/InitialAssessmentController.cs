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
    [RoutePrefix("InitialAssessment")]
    public class InitialAssessmentController : ApiController
    {
        private readonly IInitialAssessmentService _service;

        public InitialAssessmentController(IInitialAssessmentService service)
        {
            _service = service;
        }

        [HttpGet]
        [Route("GetInitialAssessment")]
        public async Task<IHttpActionResult> GetInitialAssessment()
        {
            var result = await _service.GetInitialAssessment();
            return Ok(result);
        }

        [HttpPost]
        [Route("SaveAssessmentAnswer")]
        public async Task<IHttpActionResult> SaveAssessmentAnswer(IEnumerable<AssessmentAnswerInputViewModel> input)
        {
            var result = await _service.SaveAssessmentAnswer( input);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetInitialAssessmentAnswer")]
        public async Task<IHttpActionResult> GetInitialAssessmentAnswer(int sequenceNumber, bool isActive)
        {
            var result = await _service.GetInitialAssessmentAnswer(sequenceNumber, isActive);
            return Ok(result);
        }
    }
}
