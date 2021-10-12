using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using YourSensei.Service;
using YourSensei.ViewModel;
using YourSensei.ViewModel.TrainingEvents;

namespace YourSensei.Controllers
{
    [Authorize]
    [RoutePrefix("TrainingEvent")]
    public class TrainingEventController : ApiController
    {
        private readonly ITrainingEventService _service;

        public TrainingEventController(TrainingEventService service)
        {
            _service = service;
        }

        [HttpGet]
        [Route("GetEvent")]
        public async Task<IHttpActionResult> GetEvent(string companyID, string userDetailID, bool isIndividual)
        {
            var result = await _service.GetEvent(companyID,userDetailID, isIndividual);           
            return Ok(result);
        }

        [HttpPost]
        [Route("CreateEvent")]
        public async Task<IHttpActionResult> CreateEvent(CreateEventInputViewModel inputobj)
        {
            var result = await _service.CreateEvent(inputobj);
            return Ok(result);
        }

        [HttpPost]
        [Route("CloseEvent")]
        public async Task<IHttpActionResult> CloseEvent(string closeNote, string eventId)
        { 
            var result = await _service.CloseEvent( closeNote,  eventId);
            return Ok(result);
        }      

        [HttpGet]
        [Route("GetEventFormat")]
        public async Task<IHttpActionResult> GetEventFormat()
        {
            var result = await _service.GetEventFormat();
            return Ok(result);
        }
        //[AllowAnonymous]
        [HttpPost]
        [Route("GetEventById")]
        public async Task<IHttpActionResult> GetEventById(string id)
        {
            var result = await _service.GetEventById(id);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetSelectEmployeeToAttendTrainingEvent")]
        public async Task<IHttpActionResult> GetSelectEmployeeToAttendTrainingEvent(string traingeventid, string companyId)
        {
            var result = await _service.GetSelectEmployeeToAttendTrainingEvent( traingeventid,  companyId);
            return Ok(result);
        }
       
        [HttpGet]
        [Route("GetSelectedEmployeeToAttendTrainingEvent")]
        public async Task<IHttpActionResult> GetSelectedEmployeeToAttendTrainingEvent(string traingeventid, bool isActive)
        {
            var result = await _service.GetSelectedEmployeeToAttendTrainingEvent( traingeventid, isActive);
            return Ok(result);
        }

        [HttpPost]
        [Route("CreateEmployeeToEventAttendee")]
        public async Task<IHttpActionResult> CreateEmployeeToEventAttendee(IEnumerable<TrainingEventAttendeeInputViewModel> inputobj)
        {
            var result = await _service.CreateEmployeeToEventAttendee( inputobj);
            return Ok(result);
        }
        [HttpPost]
        [Route("UpdateAttendeelogsbyAtendeeid")]
        public async Task<IHttpActionResult> UpdateAttendeelogsbyAtendeeid(IEnumerable<TrainingEventAttendeeInputViewModel> inputobj)
        {
            var result = await _service.UpdateAttendeelogsbyAtendeeid(inputobj);
            return Ok(result);
        }

       
        [HttpPost]
        [Route("DeleteEvent")]
        public async Task<IHttpActionResult> DeleteEvent(string id)
        {
            var result = await _service.DeleteEvent( id);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetTrainingEventAttendeeByMentorIDAndEmployeeID")]
        public async Task<IHttpActionResult> GetTrainingEventAttendeeByMentorIDAndEmployeeID(string mentorID, bool isActive, string employeeID)
        {
            var result = await _service.GetTrainingEventAttendeeByMentorIDAndEmployeeID(mentorID, isActive, employeeID);
            return Ok(result);
        }

        [HttpGet]
        [Route("IsInvitedToTrainingEvent")]
        public async Task<IHttpActionResult> IsInvitedToTrainingEvent(string trainingEventID, string employeeID)
        {
            var result = await _service.IsInvitedToTrainingEvent(trainingEventID, employeeID);
            return Ok(result);
        }
        [HttpPost]
        [Route("SaveA3FormFields")]
        public async Task<IHttpActionResult> SaveA3FormFields(A3FormViewModel input)
        {
            var result = await _service.SaveA3FormFields(input);
            return Ok(result);
        }
        [HttpPost]
        [Route("ApproveEventbyMentorFromEventId")]
        public async Task<IHttpActionResult> ApproveEventbyMentorFromEventId(string eventid)
        {
            var result = await _service.ApproveEventbyMentorFromEventId(eventid);
            return Ok(result);
        }
        [HttpPost]
        [Route("ApproveEventbyDollarApproverFromEventId")]
        public async Task<IHttpActionResult> ApproveEventbyDollarApproverFromEventId(string eventid)
        {
            var result = await _service.ApproveEventbyDollarApproverFromEventId(eventid);
            return Ok(result);
        }
        [HttpGet]
        [Route("GetA3TrainingEventsCommData")]
        public async Task<IHttpActionResult> GetA3TrainingEventsCommData(string eventid)
        {
            var result = await _service.GetA3TrainingEventsCommData(eventid);
            return Ok(result);
        }
      
        [HttpPost]
        [Route("SaveA3TrainingEventsCommData")]
        public async Task<IHttpActionResult> SaveA3TrainingEventsCommData(string eventid, string userid, string message)
        {
            var result = await _service.SaveA3TrainingEventsCommData(eventid,userid,message);
            return Ok(result);
        }
        [HttpGet]
        [Route("GetA3FormDataById")]
        public async Task<IHttpActionResult> GetA3FormDataById(string eventid)
        {
            var result = await _service.GetA3FormDataById( eventid);
            return Ok(result);
        }


        [HttpGet]
        [Route("GetA3TrainingEventsByCompanyID")]
        public async Task<IHttpActionResult> GetA3TrainingEventsByCompanyID(string companyID, Boolean isActive)
        {
            var result = await _service.GetA3TrainingEventsByCompanyID(companyID, isActive);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetKaizenBoard")]
        public async Task<IHttpActionResult> GetKaizenBoard(string companyID)
        {
            var result = await _service.GetKaizenBoard(companyID);
            return Ok(result);
        }

        //Kaizen Diagram/Tools API by Saurabh -------------

        [HttpPost]
        [Route("SaveKaizenFormFields")]
        public async Task<IHttpActionResult> SaveKaizenFormFields(KaizenFormViewModel input)
        {
            var result = await _service.SaveKaizenFormFields(input);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetKaizenFormDataById")]
        public async Task<IHttpActionResult> GetKaizenFormDataById(string eventid)
        {
            var result = await _service.GetKaizenFormDataById(eventid);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetKaizenTrainingEventsByCompanyID")]
        public async Task<IHttpActionResult> GetKaizenTrainingEventsByCompanyID(string companyID, Boolean isActive)
        {
            var result = await _service.GetKaizenTrainingEventsByCompanyID(companyID, isActive);
            return Ok(result);
        }

    }
}
