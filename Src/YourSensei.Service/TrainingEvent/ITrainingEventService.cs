using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourSensei.Data;
using YourSensei.ViewModel;
using YourSensei.ViewModel.TrainingEvents;

namespace YourSensei.Service
{
    public interface ITrainingEventService
    {
        Task<List<TrainingEventListViewModel>> GetEvent(string companyID,string userDetailID, bool isIndividual);
        Task<CreateEventResponseViewModel> CreateEvent(CreateEventInputViewModel inputobj);
        Task<ResponseViewModel> CloseEvent(string closeNote, string eventId);
        Task<List<TrainingEventFormat>> GetEventFormat();
        Task<CreateEventInputViewModel> GetEventById(string id);
        Task<List<SelectEmployeeToEventResponseViewModel>> GetSelectEmployeeToAttendTrainingEvent(string traingeventid, string companyId);
        //Task<ResponseViewModel>  CreateSelectedEmployeeToEvent(IEnumerable<SelectedEmpToTrainingEventInputViewModel> inputobj);
        Task<List<SelectEmployeeToEventResponseViewModel>> GetSelectedEmployeeToAttendTrainingEvent(string traingeventid, bool isActive);
        Task<ResponseViewModel> CreateEmployeeToEventAttendee(IEnumerable<TrainingEventAttendeeInputViewModel> inputobj);
        Task<ResponseViewModel> DeleteEvent(string id);
        //Task<List<CreditLogsResponseViewModel>> GetCreditLogsByCompanyID(string companyID);
        Task<List<TrainingEventAttendeeMentorViewModel>> GetTrainingEventAttendeeByMentorIDAndEmployeeID(string mentorID, bool isActive, string employeeID);
        Task<bool> IsInvitedToTrainingEvent(string trainingEventID, string employeeID);
        Task<ResponseViewModel> SaveA3FormFields(A3FormViewModel input);
        Task<TrainingEventA3Diagram> GetA3FormDataById(string eventid);
        Task<ResponseViewModel> UpdateAttendeelogsbyAtendeeid(IEnumerable<TrainingEventAttendeeInputViewModel> inputobj);
        Task<ResponseViewModel> ApproveEventbyMentorFromEventId(string eventid);
        Task<List<A3TrainingEventViewModel>> GetA3TrainingEventsByCompanyID(string CompanyID, bool IsActive);
        Task<ResponseViewModel> ApproveEventbyDollarApproverFromEventId(string eventid);
        Task<List<A3TrainingEventCommunication>> GetA3TrainingEventsCommData(string eventid);
        Task<ResponseViewModel> SaveA3TrainingEventsCommData(string eventid, string userid, string message);
        Task<List<KaizenBoardViewModel>> GetKaizenBoard(string CompanyID);
        Task<ResponseViewModel> SaveKaizenFormFields(KaizenFormViewModel input);
        Task<TrainingEventKaizenDiagram> GetKaizenFormDataById(string eventid);
        Task<List<KaizenTrainingEventViewModel>> GetKaizenTrainingEventsByCompanyID(string CompanyID, bool IsActive);
        TrainingEventAttendee GetTrainingEventAttendeeByEmployeeId(Guid employeeId, Guid trainingeventId);


    }
}
