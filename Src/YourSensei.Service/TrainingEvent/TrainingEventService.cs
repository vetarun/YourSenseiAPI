using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourSensei.Data;
using YourSensei.Service;
using YourSensei.ViewModel;
using YourSensei.Utility;
using System.Data.SqlClient;
using YourSensei.ViewModel.TrainingEvents;
using System.Web.Script.Serialization;
using System.Configuration;

namespace YourSensei.Service
{
    public class TrainingEventService : ITrainingEventService
    {
        private readonly YourSensei_DBEntities _context;
        private readonly ICreditLogService _creditLogService;
        private readonly IEmailWorkQueueService _emailWorkQueueService;

        public TrainingEventService(YourSensei_DBEntities context, CreditLogService creditLogService, EmailWorkQueueService emailWorkQueueService)
        {
            _context = context;
            _creditLogService = creditLogService;
            _emailWorkQueueService = emailWorkQueueService;
        }

        public async Task<List<TrainingEventListViewModel>> GetEvent(string companyID, string userDetailID, bool isIndividual)
        {
            try
            {

                var eventList = isIndividual == false ? await (
         from c in _context.TrainingEvents.Where(d => d.IsActive == true)
         join p in _context.TrainingEventFormats on c.TrainingEventFormatID equals p.ID into ps
         from p in ps.DefaultIfEmpty()
         where c.CompanyID == new Guid(companyID)
         select new
         {
             Id = c.ID,
             IconUrl = p == null ? "" : p.IconUrl,
             EventsName = c.Name,
             Instructor = c.instructor,
             Duration = c.duration,
             StartDate = c.startdate,
             Location = c.location,
             Owner = c.owner,
             EventFormatId = c.TrainingEventFormatID,
             status = c.Status
         }).OrderByDescending(n => n.StartDate).ToListAsync()
         : await (
         from c in _context.TrainingEvents.Where(d => d.IsActive == true)
         join p in _context.TrainingEventFormats on c.TrainingEventFormatID equals p.ID into ps
         from p in ps.DefaultIfEmpty()
         where c.UserDetailID == new Guid(userDetailID)
         select new
         {
             Id = c.ID,
             IconUrl = p == null ? "" : p.IconUrl,
             EventsName = c.Name,
             Instructor = c.instructor,
             Duration = c.duration,
             StartDate = c.startdate,
             Location = c.location,
             Owner = c.owner,
             EventFormatId = c.TrainingEventFormatID,
             status = c.Status
         }).OrderByDescending(n => n.StartDate).ToListAsync();




                List<TrainingEventListViewModel> rtnobj = new List<TrainingEventListViewModel>();
                foreach (var item in eventList)
                {
                    Employee employee = _context.Employees.Where(a => a.ID == item.Instructor).FirstOrDefault();
                    Mentor mentor = _context.Mentors.Where(a => a.ID == item.Instructor).FirstOrDefault();

                    rtnobj.Add(new TrainingEventListViewModel
                    {
                        Id = item.Id,
                        Duration = item.Duration,
                        EventsName = item.EventsName,
                        EventFormat = _context.TrainingEventFormats.Find(item.EventFormatId).Name,
                        IconUrl = item.IconUrl,
                        Instructor = employee != null ? employee.FirstName + " " + employee.LastName : mentor.FirstName + " " + mentor.LastName,
                        Location = item.Location,
                        Owner = item.Owner,
                        StartDate = item.StartDate,
                        status = item.status
                    });
                }

                return rtnobj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<CreateEventResponseViewModel> CreateEvent(CreateEventInputViewModel inputobj)
        {
            try
            {
                if (inputobj.id == null)
                {
                    TrainingEvent obj = new TrainingEvent();
                    obj.IsActive = true;
                    obj.Name = inputobj.eventsname;
                    obj.bannerimageurl = inputobj.bannerimageurl;
                    obj.CreatedBy = new Guid(inputobj.owner);
                    obj.CreatedDate = DateTime.UtcNow;
                    obj.instructor = new Guid(inputobj.instructor);
                    obj.duration = inputobj.duration;
                    obj.owner = new Guid(inputobj.owner);
                    obj.location = inputobj.location;
                    obj.startdate = inputobj.startdate;
                    obj.responsibletrainer = new Guid(inputobj.responsibletrainer);
                    obj.TrainingEventFormatID = new Guid(inputobj.trainingformat);
                    obj.trainingdescription = inputobj.trainingdescription;
                    obj.trainingnotes = inputobj.trainingnotes;
                    obj.Status = "Scheduled";
                    if (!inputobj.isIndividual)
                    {
                        obj.CompanyID = new Guid(inputobj.companyid);
                    }
                    else
                    {
                        obj.UserDetailID = new Guid(inputobj.userDetailedID);
                    }
                    obj.ID = Guid.NewGuid();

                    _context.TrainingEvents.Add(obj);
                    await _context.SaveChangesAsync();
                    return new CreateEventResponseViewModel { Code = 200, Message = "Your event has been created successfully", eventid = obj.ID };
                }
                else
                {
                    Guid eventid = new Guid(inputobj.id);
                    var result = await _context.TrainingEvents.FirstOrDefaultAsync(d => d.ID == eventid);

                    if (result != null)
                    {
                        result.IsActive = true;
                        result.Name = inputobj.eventsname;
                        result.bannerimageurl = inputobj.bannerimageurl;
                        result.ModifiedBy = new Guid(inputobj.owner);
                        result.ModifiedDate = DateTime.UtcNow;
                        result.instructor = new Guid(inputobj.instructor);
                        result.duration = inputobj.duration;
                        result.owner = new Guid(inputobj.owner);
                        result.location = inputobj.location;
                        result.startdate = inputobj.startdate;
                        result.responsibletrainer = new Guid(inputobj.responsibletrainer);
                        result.TrainingEventFormatID = new Guid(inputobj.trainingformat);
                        result.trainingdescription = inputobj.trainingdescription;
                        result.trainingnotes = inputobj.trainingnotes;

                        _context.SaveChanges();
                        return new CreateEventResponseViewModel { Code = 200, Message = "Your event has been updated successfully" };
                    }
                    else
                    {
                        return new CreateEventResponseViewModel { Code = 400, Message = "Your event does not exist in records!" };
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ResponseViewModel> CloseEvent(string closeNote, string eventId)
        {
            try
            {
                Guid eventid = new Guid(eventId);
                TrainingEvent trainingEvent = _context.TrainingEvents.FirstOrDefault(d => d.ID == eventid);

                var q = (
                   from c in _context.Employees.Where(t => t.IsActive == true)
                   join p in _context.TrainingEventAttendees.Where(d => d.TrainigEventID == eventid) on c.ID equals p.EmployeeID
                   select c).ToList();

                if (trainingEvent != null)
                {
                    bool isA3Event = trainingEvent.TrainingEventFormatID == new Guid("6F9F04CC-198E-479C-A93F-6C3C0A359194");
                    bool isKaizenEvent = trainingEvent.TrainingEventFormatID == new Guid("5518993a-efc0-4ad0-bcd7-beaea42cc2ce");
                    trainingEvent.ClosingNote = closeNote;
                    if (closeNote != null)
                    {


                        if (isA3Event || isKaizenEvent)
                        {
                            trainingEvent.Isclosed = false;
                            trainingEvent.Status = "Submitted";
                            await _context.SaveChangesAsync();
                            await SendCloseEventMail(eventId, trainingEvent, true);

                        }
                        else
                        {
                            trainingEvent.Isclosed = true;
                            trainingEvent.ClosedDate = DateTime.UtcNow;
                            trainingEvent.ClosedBy = trainingEvent.owner;
                            trainingEvent.Status = "Closed";

                            await _context.SaveChangesAsync();

                            await SendCloseEventMail(eventId, trainingEvent, false);

                        }

                    }

                    return new ResponseViewModel { Code = 200, Message = "Your event has been updated successfully" };
                }
                else
                {
                    return new ResponseViewModel { Code = 400, Message = "event not found" };
                }
            }
            catch (Exception ex)
            {
                return new ResponseViewModel { Code = 500, Message = ex.InnerException.Message };
            }
        }
        public async Task<bool> SendCloseEventMail(string eventId, TrainingEvent trainingEvent, bool isA3Event)
        {
            try
            {
                Guid eventid = new Guid(eventId);
                List<EmployeeResponseViewModel> employeeResponseViewModels = (from tea in _context.TrainingEventAttendees
                                                                              join e in _context.Employees on tea.EmployeeID equals e.ID
                                                                              join ud in _context.UserDetails on e.ID equals ud.EmployeeID
                                                                              where tea.TrainigEventID == eventid
                                                                              select new EmployeeResponseViewModel
                                                                              {
                                                                                  Id = e.ID.ToString(),
                                                                                  userDetialID = ud.ID.ToString(),
                                                                                  FirstName = e.FirstName,
                                                                                  LastName = e.LastName,
                                                                                  CompanyId = e.CompanyId.ToString(),
                                                                                  MentorId = e.MentorId.ToString(),
                                                                                  Email = ud.UserName,
                                                                                  EmployeeCode = e.EmployeeCode,
                                                                                  MemberID = e.MemberID
                                                                              })
                                                                                      .Union(from tea in _context.TrainingEventAttendees
                                                                                             join e in _context.Mentors on tea.EmployeeID equals e.ID
                                                                                             join ud in _context.UserDetails on e.ID equals ud.EmployeeID
                                                                                             where tea.TrainigEventID == eventid
                                                                                             select new EmployeeResponseViewModel
                                                                                             {
                                                                                                 Id = e.ID.ToString(),
                                                                                                 userDetialID = ud.ID.ToString(),
                                                                                                 FirstName = e.FirstName,
                                                                                                 LastName = e.LastName,
                                                                                                 CompanyId = "00000000-0000-0000-0000-000000000000",
                                                                                                 MentorId = "",
                                                                                                 Email = ud.UserName,
                                                                                                 EmployeeCode = "",
                                                                                                 MemberID = e.MemberID
                                                                                             }).ToList();
                Employee trainingEventCreatorEmployee = _context.Employees.Where(a => a.ID == trainingEvent.owner).FirstOrDefault();
                if (trainingEventCreatorEmployee == null)
                    trainingEventCreatorEmployee = (from e in _context.Employees
                                                    join ud in _context.UserDetails on e.ID equals ud.EmployeeID
                                                    where ud.ID == trainingEvent.owner && e.IsActive == true
                                                    select e).FirstOrDefault();

                if (isA3Event == false)
                {

                    foreach (var item in employeeResponseViewModels)
                    {
                        var eventdet = _context.TrainingEventAttendees.FirstOrDefault(x => x.EmployeeID == new Guid(item.Id) &&
                            x.TrainigEventID == eventid);
                        decimal credit = Convert.ToDecimal((eventdet.Time * eventdet.Test) / 100);
                        await _creditLogService.SaveCreditLog(item, credit, eventId, "TrainingEvent");

                        CloseEventMailToAttViewModel closeEventMailToAtt = new CloseEventMailToAttViewModel
                        {
                            Credit=credit,
                            EventCreator=trainingEventCreatorEmployee.FirstName+" "+trainingEventCreatorEmployee.LastName,
                            EventName=trainingEvent.Name,
                            FullName=item.FirstName+" "+item.LastName,
                            ToEmail=item.Email,
                            ClosingNote=trainingEvent.ClosingNote
                        };

                        EmailWorkQueue emailWorkQueue = new EmailWorkQueue()
                        {
                            WorkItemType = "CloseEventEmailToAttendee",
                            KeyID = eventId,
                            KeyType = "TrainingEvent",
                            SendToEmployee = trainingEventCreatorEmployee.ID,
                            Subject = "Event Submitted " + trainingEvent.Name,
                            Body = "",
                            Template = "CloseEventEmailToAttendee.html",
                            TemplateContent = new JavaScriptSerializer().Serialize(closeEventMailToAtt),
                            Status = "Pending",
                            CreatedDate = DateTime.UtcNow,
                            ModifiedDate = DateTime.UtcNow
                        };
                        await _emailWorkQueueService.Save(emailWorkQueue);
                        //Attendee mail
                        //SendMail.SendCloseEventEmailToAttendee(trainingEvent, item, trainingEventCreatorEmployee, credit);

                        item.Credit = credit;
                    }
                    IEnumerable<IGrouping<string, EmployeeResponseViewModel>> MentorGroup = employeeResponseViewModels.
                                    Where(x => !String.IsNullOrWhiteSpace(x.MentorId)).GroupBy(r => r.MentorId);
                    foreach (var item in MentorGroup)
                    {
                        Employee employee = new Employee();
                        var empDetails = _context.Employees.Find(new Guid(item.Key));
                        var empDetilsfrommentor = _context.Mentors.Find(new Guid(item.Key));
                        employee.Email = empDetails == null ? empDetilsfrommentor.Email : empDetails.Email;
                        employee.FirstName = empDetails == null ? empDetilsfrommentor.FirstName : empDetails.FirstName;
                        employee.LastName = empDetails == null ? empDetilsfrommentor.LastName : empDetails.LastName;

                        //Mentor mail
                        List<EmployeeResponseViewModel> empList = item.ToList();
                        CloseEventEmailViewModel closeEventEmail = new CloseEventEmailViewModel()
                        {
                            FullName=employee.FirstName+" "+employee.LastName,
                            TrainingEventCreator=trainingEventCreatorEmployee.FirstName+" "+trainingEventCreatorEmployee.LastName,
                            TrainingEventName=trainingEvent.Name,
                            ToEmail=employee.Email
                        };

                        EmailWorkQueue emailWorkQueue = new EmailWorkQueue()
                        {
                            WorkItemType = "CloseEventEmailToMentor",
                            KeyID = eventId,
                            KeyType = "TrainingEvent",
                            SendToEmployee = trainingEventCreatorEmployee.ID,
                            Subject = "Event Closed " + trainingEvent.Name,
                            Body = "",
                            Template = "CloseEventEmailToMentor.html",
                            TemplateContent = new JavaScriptSerializer().Serialize(closeEventEmail),
                            Status = "Pending",
                            CreatedDate = DateTime.UtcNow,
                            ModifiedDate = DateTime.UtcNow
                        };
                        await _emailWorkQueueService.Save(emailWorkQueue);
                        //SendMail.SendCloseEventEmail(empList, employee, trainingEvent, trainingEventCreatorEmployee, isA3Event);
                    }

                    //Instructor mail
                    Employee instructorEmployee = null;
                    instructorEmployee = _context.Employees.Where(a => a.ID == trainingEvent.instructor).FirstOrDefault();
                    if (instructorEmployee == null)
                    {
                        Mentor mentor = _context.Mentors.Where(a => a.ID == trainingEvent.instructor).FirstOrDefault();
                        instructorEmployee = new Employee();
                        instructorEmployee.FirstName = mentor.FirstName;
                        instructorEmployee.LastName = mentor.LastName;
                        instructorEmployee.Email = mentor.Email;

                        CloseEventEmailViewModel closeEventEmail = new CloseEventEmailViewModel()
                        {
                            FullName = instructorEmployee.FirstName + " " + instructorEmployee.LastName,
                            TrainingEventCreator = trainingEventCreatorEmployee.FirstName + " " + trainingEventCreatorEmployee.LastName,
                            TrainingEventName = trainingEvent.Name,
                            ToEmail=instructorEmployee.Email
                        };

                        EmailWorkQueue emailWorkQueue = new EmailWorkQueue()
                        {
                            WorkItemType = "CloseEventEmail",
                            KeyID = eventId,
                            KeyType = "TrainingEvent",
                            SendToEmployee = trainingEventCreatorEmployee.ID,
                            Subject = "Event closed" + trainingEvent.Name,
                            Body = "",
                            Template = "CloseEventEmail.html",
                            TemplateContent = new JavaScriptSerializer().Serialize(closeEventEmail),
                            Status = "Pending",
                            CreatedDate = DateTime.UtcNow,
                            ModifiedDate = DateTime.UtcNow
                        };
                        await _emailWorkQueueService.Save(emailWorkQueue);
                    }
                    // SendMail.SendCloseEventEmail(employeeResponseViewModels, instructorEmployee, trainingEvent, trainingEventCreatorEmployee, isA3Event);

                    //Company admin mail
                    string userCategoryDescription = EnumHelper.GetDescription(Utility.UserCategory.CompanyAdmin);
                    List<Employee> adminEmployees = (from e in _context.Employees
                                                     join ud in _context.UserDetails on e.ID equals ud.EmployeeID
                                                     join uc in _context.UserCategories on ud.UserType equals uc.Id
                                                     where e.CompanyId == trainingEvent.CompanyID &&
                                                        e.IsActive == true &&
                                                        uc.Description == userCategoryDescription
                                                     select e).ToList();
                    foreach (Employee employee in adminEmployees)
                    {
                        CloseEventEmailViewModel closeEventEmail = new CloseEventEmailViewModel()
                        {
                            FullName = employee.FirstName + " " + employee.LastName,
                            TrainingEventCreator = trainingEventCreatorEmployee.FirstName + " " + trainingEventCreatorEmployee.LastName,
                            TrainingEventName = trainingEvent.Name,
                            ToEmail=employee.Email
                        };

                        EmailWorkQueue emailWorkQueue = new EmailWorkQueue()
                        {
                            WorkItemType = "CloseEventEmail",
                            KeyID = eventId,
                            KeyType = "TrainingEvent",
                            SendToEmployee = trainingEventCreatorEmployee.ID,
                            Subject = "Event Closed " + trainingEvent.Name,
                            Body = "",
                            Template = "CloseEventEmail.html",
                            TemplateContent = new JavaScriptSerializer().Serialize(closeEventEmail),
                            Status = "Pending",
                            CreatedDate = DateTime.UtcNow,
                            ModifiedDate = DateTime.UtcNow
                        };
                        await _emailWorkQueueService.Save(emailWorkQueue);
                        // SendMail.SendCloseEventEmail(employeeResponseViewModels, employee, trainingEvent, trainingEventCreatorEmployee, isA3Event);

                    }
                }
                else
                {
                    IEnumerable<IGrouping<string, EmployeeResponseViewModel>> MentorGroup = employeeResponseViewModels.
                                   Where(x => !String.IsNullOrWhiteSpace(x.MentorId)).GroupBy(r => r.MentorId);
                    foreach (var item in MentorGroup)
                    {
                        Employee employee = new Employee();
                        var empDetails = _context.Employees.Find(new Guid(item.Key));
                        var empDetilsfrommentor = _context.Mentors.Find(new Guid(item.Key));
                        employee.Email = empDetails == null ? empDetilsfrommentor.Email : empDetails.Email;
                        employee.FirstName = empDetails == null ? empDetilsfrommentor.FirstName : empDetails.FirstName;
                        employee.LastName = empDetails == null ? empDetilsfrommentor.LastName : empDetails.LastName;

                        CloseA3EvtMailToAttViewModel closeA3EvtMailToAtt = new CloseA3EvtMailToAttViewModel() { 
                            FullName=employee.FirstName+" "+employee.LastName,
                            ToEmail=employee.Email,
                            TrainingEventCreator= trainingEventCreatorEmployee.FirstName+" "+ trainingEventCreatorEmployee.LastName,
                            TrainingEventName=trainingEvent.Name
                        };
                        EmailWorkQueue emailWorkQueue = new EmailWorkQueue()
                        {
                            WorkItemType = "CloseA3EventEmailToAttendee",
                            KeyID = eventId,
                            KeyType = "TrainingEvent",
                            SendToEmployee = Guid.Empty,
                            Subject = "Event Submitted " + trainingEvent.Name,
                            Body = "",
                            Template = "CloseA3EventEmailToAttendee.html",
                            TemplateContent = new JavaScriptSerializer().Serialize(closeA3EvtMailToAtt),
                            Status = "Pending",
                            CreatedDate = DateTime.UtcNow,
                            ModifiedDate = DateTime.UtcNow
                        };
                        await _emailWorkQueueService.Save(emailWorkQueue);

                        //Mentor mail
                        List<EmployeeResponseViewModel> empList = item.ToList();
                        //SendMail.SendCloseEventEmail(empList, employee, trainingEvent, trainingEventCreatorEmployee, isA3Event);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public  TrainingEventAttendee GetTrainingEventAttendeeByEmployeeId( Guid employeeId, Guid trainingeventId )
        {
            try
            {
                TrainingEventAttendee eventdet = _context.TrainingEventAttendees.FirstOrDefault(x => x.EmployeeID == employeeId &&
                           x.TrainigEventID == trainingeventId);

                return eventdet;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<TrainingEventFormat>> GetEventFormat()
        {
            try
            {
                var result = await _context.TrainingEventFormats.Where(d => d.IsActive == true).ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<CreateEventInputViewModel> GetEventById(string id)
        {
            Guid eventid = new Guid(id);
            try
            {
                var result = await _context.TrainingEvents.SingleOrDefaultAsync(d => d.ID == eventid);
                Employee employee = (from e in _context.Employees
                                     join ud in _context.UserDetails on e.ID equals ud.EmployeeID
                                     where ud.ID == result.responsibletrainer && e.IsActive == true
                                     select e).FirstOrDefault();

                Employee instructorEmployee = _context.Employees.Where(a => a.ID == result.instructor && a.IsActive == true).FirstOrDefault();
                Mentor instructorMentor = _context.Mentors.Where(a => a.ID == result.instructor && a.IsActive == true).FirstOrDefault();

                CreateEventInputViewModel obj = new CreateEventInputViewModel
                {
                    responsibleTrainerEmployeeID = employee.ID.ToString(),
                    responsibletrainer = Convert.ToString(result.responsibletrainer),
                    responsibleTrainerName = employee.FirstName + " " + employee.LastName,
                    trainingformat = Convert.ToString(result.TrainingEventFormatID),
                    trainingFormatName = _context.TrainingEventFormats.Where(a => a.ID == result.TrainingEventFormatID).Select(a => a.Name).SingleOrDefault(),
                    eventsname = result.Name,
                    location = result.location,
                    duration = result.duration,
                    instructor = Convert.ToString(result.instructor),
                    instructorName = instructorEmployee != null ? instructorEmployee.FirstName + " " + instructorEmployee.LastName :
                        instructorMentor.FirstName + " " + instructorMentor.LastName,
                    //plannedtrainingdate = result.plannedtrainingdate,
                    trainingdescription = result.trainingdescription,
                    trainingnotes = result.trainingnotes,
                    startdate = result.startdate,
                    //enddate = result.enddate,                    
                    IsClosed = result.Isclosed.GetValueOrDefault(),
                    ClosingNote = result.ClosingNote,
                    companyid = result.CompanyID.ToString(),
                    ClosedDate = result.ClosedDate,
                    status = result.Status
                };

                return obj;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<SelectEmployeeToEventResponseViewModel>> GetSelectEmployeeToAttendTrainingEvent(string traingeventid, string companyId)
        {
            Guid eventid = new Guid(traingeventid);
            try
            {
                DateTime mindate = System.DateTime.MinValue;
                var q = await (
        from c in _context.Employees.Where(c => c.IsActive == true)
        join p in _context.TrainingEventAttendees.Where(d => d.TrainigEventID == eventid) on c.ID equals p.EmployeeID into ps
        from p in ps.DefaultIfEmpty()
        where c.CompanyId == new Guid(companyId)
        select new SelectEmployeeToEventResponseViewModel
        {
            EmpId = c.ID,
            EmployeeName = c.FirstName + " " + c.LastName,
            Isselected = p == null ? false : true,
            Test = p.Test,
            Time = p.Time

        }).OrderBy(n => n.EmployeeName).ToListAsync();

                return q;
            }
            catch
            {
                throw;
            }
        }

        //public async Task<List<SelectEmployeeToEventResponseViewModel>> GetSelectedEmployeeToAttendTrainingEvent(string traingeventid, string companyId)
        //{
        //    Guid eventid = new Guid(traingeventid);
        //    try
        //    {
        //        DateTime mindate = System.DateTime.MinValue;
        //        TrainingEvent trainingEvent =  _context.TrainingEvents.Where(eve => eve.ID == eventid).FirstOrDefault();
        //        List<SelectEmployeeToEventResponseViewModel> q = await (
        //        from c in _context.Employees
        //        join p in _context.TrainingEventAttendees.Where(d => d.TrainigEventID == eventid) on c.ID equals p.EmployeeID
        //        join e in _context.TrainingEvents.Where(eve => eve.ID == eventid) on p.TrainigEventID equals e.ID
        //        select new SelectEmployeeToEventResponseViewModel
        //        {
        //            EmpId = c.ID,
        //            EmployeeName = c.FirstName + " " + c.LastName,
        //            EventTrainingName = p == null ? "(No Events)" : e.Name,
        //            Duration = p == null ? "(No Data)" : e.duration,
        //            ScheduleDate = p == null ? System.DateTime.MinValue : e.startdate,
        //            Test = e.Isclosed == true ? p.Test : p.Test == 0 ? 100 : p.Test,
        //            Time = p.Time
        //        }).OrderBy(n => n.EmployeeName).ToListAsync();

        //        foreach (SelectEmployeeToEventResponseViewModel att in q)
        //        {
        //            att.Time = trainingEvent.Isclosed == true ? att.Time : att.Time == 0 ? Convert.ToDecimal(trainingEvent.duration) : att.Time;
        //        }

        //        return q;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        public async Task<List<SelectEmployeeToEventResponseViewModel>> GetSelectedEmployeeToAttendTrainingEvent(string traingeventid, bool isActive)
        {
            try
            {
                Guid TrainingEventID = new Guid(traingeventid);
                List<SelectEmployeeToEventResponseViewModel> companyLibraryBooks = await _context.Database.SqlQuery<SelectEmployeeToEventResponseViewModel>(
                    "dbo.usp_GetTrainingEventAttendeeByID @TrainigEventID = @trainingEventID, @IsActive = @isActive",
                    new SqlParameter("trainingEventID", traingeventid),
                    new SqlParameter("isActive", isActive)).ToListAsync();

                return companyLibraryBooks;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ResponseViewModel> CreateEmployeeToEventAttendee(IEnumerable<TrainingEventAttendeeInputViewModel> inputobj)
        {
            try
            {
                //List<TrainingEventAttendee> emp = new List<TrainingEventAttendee>();
                var eventid = inputobj.FirstOrDefault();
                var result = await _context.TrainingEventAttendees.Where(d => d.TrainigEventID == new Guid(eventid.TrainigEventID)).ToListAsync();
                _context.TrainingEventAttendees.RemoveRange(result);
                await _context.SaveChangesAsync();

                TrainingEvent trainingEvent = _context.TrainingEvents.Where(a => a.ID == new Guid(eventid.TrainigEventID)).FirstOrDefault();
                if (trainingEvent != null)
                {
                    Employee trainingEventCreatorEmployee = _context.Employees.Where(a => a.ID == trainingEvent.owner).FirstOrDefault();
                    if (trainingEventCreatorEmployee == null)
                        trainingEventCreatorEmployee = (from e in _context.Employees
                                                        join ud in _context.UserDetails on e.ID equals ud.EmployeeID
                                                        where ud.ID == trainingEvent.owner && e.IsActive == true
                                                        select e).FirstOrDefault();

                    Employee instructorEmployee = new Employee();

                    instructorEmployee = _context.Employees.Where(a => a.ID == trainingEvent.instructor).FirstOrDefault();

                    if (instructorEmployee == null)
                    {
                        instructorEmployee = new Employee();
                        Mentor mentor = _context.Mentors.Where(a => a.ID == trainingEvent.instructor).FirstOrDefault();
                        instructorEmployee.FirstName = mentor.FirstName;
                        instructorEmployee.LastName = mentor.LastName;
                        instructorEmployee.Email = mentor.Email;
                    }

                    List<Employee> attendeeEmployees = new List<Employee>();

                    foreach (var item in inputobj)
                    {
                        TrainingEventAttendee trainingEventAttendee = new TrainingEventAttendee()
                        {
                            EmployeeID = new Guid(item.EmployeeID),
                            TrainigEventID = new Guid(item.TrainigEventID),
                            Test = item.Test,
                            Time = item.Time,
                            ID = Guid.NewGuid()
                        };
                        _context.TrainingEventAttendees.Add(trainingEventAttendee);
                        _context.SaveChanges();

                        //Attendee mail
                        Employee employee = _context.Employees.Where(a => a.ID == new Guid(item.EmployeeID)).FirstOrDefault();
                        if (employee != null)
                        {
                            TrainingEventInvitationMailViewModel trainingEventInvitationMail = new TrainingEventInvitationMailViewModel()
                            {
                                FullName=employee.FirstName+" "+employee.LastName,
                                Instructor=instructorEmployee.FirstName+" "+instructorEmployee.LastName,
                                ScheduleDate=trainingEvent.startdate,
                                TrainingEventCreator= trainingEventCreatorEmployee.FirstName+" "+ trainingEventCreatorEmployee.LastName,
                                TrainingEventName=trainingEvent.Name,
                                TrainingNote=trainingEvent.trainingnotes,
                                EmployeeEmail=employee.Email
                            };
                            EmailWorkQueue emailWorkQueue = new EmailWorkQueue()
                            {
                                WorkItemType = "TrainingEventInvitationEmail",
                                KeyID = item.TrainigEventID,
                                KeyType = "TrainingEvent",
                                SendToEmployee = employee.ID,
                                Subject = "Invitation - " + trainingEvent.Name,
                                Body = "",
                                Template = "TrainingEventInvitationEmail.html",
                                TemplateContent = new JavaScriptSerializer().Serialize(trainingEventInvitationMail),
                                Status = "Pending",
                                CreatedDate = DateTime.UtcNow,
                                ModifiedDate = DateTime.UtcNow
                            };
                            await _emailWorkQueueService.Save(emailWorkQueue);
                            //SendMail.SendTrainingEventInvitationEmail(trainingEvent, employee, trainingEventCreatorEmployee, instructorEmployee);

                            attendeeEmployees.Add(employee);
                        }
                    }

                    var q = (
                       from c in _context.Employees.Where(t => t.IsActive == true)
                       join p in _context.TrainingEventAttendees.Where(d => d.TrainigEventID == new Guid(eventid.TrainigEventID)) on c.ID equals p.EmployeeID
                       select c).ToList();

                    IEnumerable<IGrouping<Guid?, Employee>> MentorGroup = q.Where(x => x.MentorId != null).GroupBy(r => r.MentorId);
                    foreach (var item in MentorGroup)
                    {
                        Employee employee = new Employee();
                        var empDetails = _context.Employees.Find(item.Key);
                        var empDetilsfrommentor = _context.Mentors.Find(item.Key);
                       employee.Email = empDetails == null ? empDetilsfrommentor.Email : empDetails.Email;
                       employee.FirstName = empDetails == null ? empDetilsfrommentor.FirstName : empDetails.FirstName;
                       employee.LastName = empDetails == null ? empDetilsfrommentor.LastName : empDetails.LastName;

                        //Mentor mail
                        List<Employee> empList = item.ToList();

                        NewEventNotificationViewmodel newEventNotificationViewmodel = new NewEventNotificationViewmodel()
                        {
                            ToEmployeeEmail = employee.Email.ToString(),
                            ToEmployeeName = employee.FirstName + " " + employee.LastName,
                            TrainingEventCreator = trainingEventCreatorEmployee.FirstName + " " + trainingEventCreatorEmployee.LastName,
                            TrainingEventName = trainingEvent.Name, 
                            Instructor = instructorEmployee.FirstName + " " + instructorEmployee.LastName,
                            ScheduledDate = trainingEvent.startdate.ToString(),
                            TrainingNotes = trainingEvent.trainingnotes,
                            StudentsInvited = GetEmpTable(empList).ToString()
                        };
                        EmailWorkQueue emailWorkQueue = new EmailWorkQueue()
                        {
                            WorkItemType = "NewEventNotificationEmailToMentor",
                            KeyID = eventid.TrainigEventID,
                            KeyType = "TrainingEvent",
                            SendToEmployee = Guid.Empty,
                            Subject = "Notification - " + trainingEvent.Name,
                            Body = "",
                            Template = "NewEventNotificationEmail.html",
                            TemplateContent = new JavaScriptSerializer().Serialize(newEventNotificationViewmodel),
                            Status = "Pending",
                            CreatedDate = DateTime.UtcNow,
                            ModifiedDate = DateTime.UtcNow
                        };
                        await _emailWorkQueueService.Save(emailWorkQueue);
                        // SendMail.SendNewEventNotificationEmail(empList, employee, trainingEvent, trainingEventCreatorEmployee, instructorEmployee);
                    }

                    //Instructor mail
                    if (instructorEmployee != null)
                    {
                        NewEventNotificationViewmodel newEventNotificationViewmodel = new NewEventNotificationViewmodel()
                        {
                            ToEmployeeEmail = instructorEmployee.Email.ToString(),
                            ToEmployeeName = instructorEmployee.FirstName + " " + instructorEmployee.LastName,
                            TrainingEventCreator = trainingEventCreatorEmployee.FirstName + " " + trainingEventCreatorEmployee.LastName,
                            TrainingEventName = trainingEvent.Name,
                            Instructor = "",// instructorEmployee.FirstName + " " + instructorEmployee.LastName,
                            ScheduledDate = trainingEvent.startdate.ToString(),
                            TrainingNotes = trainingEvent.trainingnotes,
                            StudentsInvited = GetEmpTable(attendeeEmployees).ToString()
                        };

                        EmailWorkQueue emailWorkQueue1 = new EmailWorkQueue()
                            {
                                WorkItemType = "NewEventNotificationEmailToMentor",//for instructor
                                KeyID = eventid.TrainigEventID,
                                KeyType = "TrainingEvent",
                                SendToEmployee = instructorEmployee.ID,
                                Subject = "Notification - " + trainingEvent.Name,
                                Body = "",
                                Template = "NewEventNotificationEmail.html",
                                TemplateContent = new JavaScriptSerializer().Serialize(newEventNotificationViewmodel),
                                //TemplateContent = "",
                                Status = "Pending",
                                CreatedDate = DateTime.UtcNow,
                                ModifiedDate = DateTime.UtcNow
                            };
                            await _emailWorkQueueService.Save(emailWorkQueue1);
                        

                    }
                     // SendMail.SendNewEventNotificationEmail(attendeeEmployees, instructorEmployee, trainingEvent, trainingEventCreatorEmployee);

                    //Company admin mail
                    string userCategoryDescription = EnumHelper.GetDescription(Utility.UserCategory.CompanyAdmin);
                    List<Employee> adminEmployees = (from e in _context.Employees
                                                     join ud in _context.UserDetails on e.ID equals ud.EmployeeID
                                                     join uc in _context.UserCategories on ud.UserType equals uc.Id
                                                     where e.CompanyId == trainingEvent.CompanyID &&
                                                        e.IsActive == true &&
                                                        uc.Description == userCategoryDescription
                                                     select e).ToList();
                    foreach (Employee employee in adminEmployees) 
                        if(adminEmployees != null)
                        {

                            NewEventNotificationViewmodel newEventNotificationViewmodel = new NewEventNotificationViewmodel()
                            {
                                ToEmployeeEmail = employee.Email.ToString(),
                                ToEmployeeName = employee.FirstName + " " + employee.LastName,
                                TrainingEventCreator = trainingEventCreatorEmployee.FirstName + " " + trainingEventCreatorEmployee.LastName,
                                TrainingEventName = trainingEvent.Name,
                                Instructor = instructorEmployee.FirstName + " " + instructorEmployee.LastName,
                                ScheduledDate = trainingEvent.startdate.ToString(),
                                TrainingNotes = trainingEvent.trainingnotes,
                                StudentsInvited = GetEmpTable(attendeeEmployees).ToString()
                            };


                            EmailWorkQueue emailWorkQueue1 = new EmailWorkQueue()
                                    {
                                        WorkItemType = "NewEventNotificationEmailToMentor",//company admin
                                        KeyID = eventid.TrainigEventID,
                                        KeyType = "TrainingEvent",
                                        SendToEmployee =Guid.Empty,
                                        Subject = "Notification - " + trainingEvent.Name,
                                        Body = "",
                                        Template = "NewEventNotificationEmail.html",
                                        TemplateContent = new JavaScriptSerializer().Serialize(newEventNotificationViewmodel),
                                       
                                        Status = "Pending",
                                        CreatedDate = DateTime.UtcNow,
                                        ModifiedDate = DateTime.UtcNow
                                    };
                                    await _emailWorkQueueService.Save(emailWorkQueue1);
                                

                        }
                        //SendMail.SendNewEventNotificationEmail(attendeeEmployees, employee, trainingEvent, trainingEventCreatorEmployee, instructorEmployee);
                }

                return new ResponseViewModel { Code = 200, Message = "Attendee added successfully!" };
            }
            catch (Exception ex)
            {
                return new ResponseViewModel { Code = 400, Message = "Something went wrong please try after a moment" };
            }
        }

        public async Task<ResponseViewModel> UpdateAttendeelogsbyAtendeeid(IEnumerable<TrainingEventAttendeeInputViewModel> inputobj)
        {
            try
            {
                foreach (var item in inputobj)
                {
                    var isAttendeeIdExist = await _context.TrainingEventAttendees.FindAsync(new Guid(item.trainingEventAttendeeID));
                    if (isAttendeeIdExist != null)
                    {
                        isAttendeeIdExist.Test = item.Test;
                        isAttendeeIdExist.Time = item.Time;
                        await _context.SaveChangesAsync();
                    }
                }
                return new ResponseViewModel { Code = 200, Message = "Your log record has been successfully saved!" };
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<ResponseViewModel> DeleteEvent(string id)
        {
            try
            {
                var result = await _context.TrainingEvents.FindAsync(new Guid(id));
                var eventAttendee = await _context.TrainingEventAttendees.Where(x => x.TrainigEventID == result.ID).ToListAsync();
                if (eventAttendee.Count != 0)
                {
                    _context.TrainingEventAttendees.RemoveRange(eventAttendee);
                    await _context.SaveChangesAsync();
                }
                result.IsActive = false;
                _context.SaveChanges();
                return new ResponseViewModel { Code = 200, Message = "Your event record has been deleted successfully!" };
            }
            catch
            {
                return new ResponseViewModel { Code = 400, Message = "Something went wrong please try again after a moment!" };
            }

        }

        public async Task<List<TrainingEventAttendeeMentorViewModel>> GetTrainingEventAttendeeByMentorIDAndEmployeeID(string mentorID, bool isActive, string employeeID)
        {
            try
            {
                Guid MentorID = new Guid(mentorID);
                Guid EmployeeID = new Guid(employeeID);
                List<TrainingEventAttendeeMentorViewModel> companyLibraryBooks = await _context.Database.SqlQuery<TrainingEventAttendeeMentorViewModel>(
                    "dbo.usp_GetTrainingEventAttendeeByMentorIDAndEmployeeID @MentorID = @mentorID, @IsActive = @isActive, @EmployeeID = @employeeID",
                    new SqlParameter("mentorID", MentorID),
                    new SqlParameter("isActive", isActive),
                    new SqlParameter("employeeID", employeeID)).ToListAsync();

                return companyLibraryBooks;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> IsInvitedToTrainingEvent(string trainingEventID, string employeeID)
        {
            try
            {
                bool result = await (from tea in _context.TrainingEventAttendees
                                     join m in _context.Mentors on tea.EmployeeID equals m.ID
                                     where tea.TrainigEventID == new Guid(trainingEventID) && tea.EmployeeID == new Guid(employeeID)
                                     select tea).AnyAsync();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ResponseViewModel> SaveA3FormFields(A3FormViewModel input)
        {
            try
            {

                Employee eventCreator = await (from e in _context.Employees
                                               join ud in _context.UserDetails on e.ID equals ud.EmployeeID
                                               where ud.ID == new Guid(input.userid)
                                               select e).FirstOrDefaultAsync();
                if (input.id == null)
                {
                    TrainingEventA3Diagram field = new TrainingEventA3Diagram
                    {
                        TrainingEventID = new Guid(input.TrainingEventID),
                        Background = input.Background ?? "",
                        CurrentCondition = input.CurrentCondition ?? "",
                        Analyses = input.Analyses ?? "",
                        IsActive = true,
                        FollowUp = input.FollowUp ?? "",
                        Goal = input.Goal ?? "",
                        Plan = input.Plan ?? "",
                        Proposal = input.Proposal ?? "",
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = new Guid(input.userid),
                        DollarImpacted = input.DollarImpacted,
                        IsDollarValueApproved = false,
                        DollarValueApprovedBy = Guid.Empty,
                        DollarValueApprovedDate = DateTime.UtcNow,
                        AssignedTo = new Guid(input.AssignedTo),
                        
                    };
                    _context.TrainingEventA3Diagram.Add(field);
                    await _context.SaveChangesAsync();

                    //TrainingEventAttendee field2 = new TrainingEventAttendee
                    //{
                    //    TrainigEventID = new Guid(input.TrainingEventID),
                    //    EmployeeID = new Guid(input.AssignedTo),
                    //    Time = 0,
                    //    Test = 0
                    //};
                    //_context.TrainingEventAttendees.Add(field2);
                    //await _context.SaveChangesAsync();

                    if (eventCreator != null && !string.IsNullOrWhiteSpace(input.mentorEmail))
                    {
                        SendA3FormUpdateNotifyEmail sendA3FormUpdateNotifyEmail = new SendA3FormUpdateNotifyEmail()
                        {
                           receiverEmail = input.mentorEmail,
                           receivername = input.mentorName,
                           FirstName = eventCreator.FirstName,
                           LastName = eventCreator.LastName,
                           Email = eventCreator.Email,
                           Analyses = field.Analyses ?? string.Empty,
                           Background = field.Background ?? string.Empty,
                           Condition = field.CurrentCondition ?? string.Empty,
                           DollarImpacted = Convert.ToString(field.DollarImpacted),
                           FollowUp = field.FollowUp ?? string.Empty,
                           Goal = field.Goal ?? string.Empty,
                           Plan = field.Plan ?? string.Empty,
                           Proposal=field.Proposal ?? string.Empty,
                           url = ConfigurationManager.AppSettings["AngularUrl"].ToString() + "/Main/studentevent?id=" + field.TrainingEventID
                        };
                        EmailWorkQueue emailWorkQueue = new EmailWorkQueue()
                        {
                            WorkItemType = "SendA3FormUpdateNotifyEmail",
                            KeyID = "",
                            KeyType = "",
                            SendToEmployee = Guid.Empty,
                            Subject = "A3 Tool form updated",
                            Body = "",
                            Template = "SendA3FormUpdateNotifyEmail.html",
                            TemplateContent = new JavaScriptSerializer().Serialize(sendA3FormUpdateNotifyEmail),
                            Status = "Pending",
                            CreatedDate = DateTime.UtcNow,
                            ModifiedDate = DateTime.UtcNow
                        };
                        await _emailWorkQueueService.Save(emailWorkQueue);
                    }
                    //SendMail.SendA3FormUpdateNotifyEmail(eventCreator, field, input.mentorEmail,input.mentorName);


                    return new ResponseViewModel { Code = 200, Message = "Records Successfully saved!" };
                }
                else
                {
                  
                    var isformidExist = await _context.TrainingEventA3Diagram.FindAsync(input.id);
                    if (isformidExist != null)
                    {
                        isformidExist.TrainingEventID = new Guid(input.TrainingEventID);
                        isformidExist.Background = input.Background ?? "";
                        isformidExist.CurrentCondition = input.CurrentCondition ?? "";
                        isformidExist.Analyses = input.Analyses ?? "";
                        isformidExist.IsActive = true;
                        isformidExist.FollowUp = input.FollowUp ?? "";
                        isformidExist.Goal = input.Goal ?? "";
                        isformidExist.Plan = input.Plan ?? "";
                        isformidExist.Proposal = input.Proposal ?? "";
                        isformidExist.Modifieddate = DateTime.UtcNow;
                        isformidExist.ModifiedBy = new Guid(input.userid);
                        isformidExist.DollarImpacted = input.DollarImpacted;
                        isformidExist.IsDollarValueApproved = false;
                        isformidExist.DollarValueApprovedBy = Guid.Empty;
                        isformidExist.DollarValueApprovedDate = DateTime.UtcNow;
                        isformidExist.AssignedTo = new Guid(input.AssignedTo);


                        await _context.SaveChangesAsync();
                        if (eventCreator != null && !string.IsNullOrWhiteSpace(input.mentorEmail))
                        {
                            SendA3FormUpdateNotifyEmail sendA3FormUpdateNotifyEmail = new SendA3FormUpdateNotifyEmail()
                            {
                                receiverEmail = input.mentorEmail,
                                receivername = input.mentorName,
                                FirstName = eventCreator.FirstName,
                                LastName = eventCreator.LastName,
                                Email = eventCreator.Email,
                                Analyses = isformidExist.Analyses ?? string.Empty,
                                Background = isformidExist.Background ?? string.Empty,
                                Condition = isformidExist.CurrentCondition ?? string.Empty,
                                DollarImpacted = Convert.ToString(isformidExist.DollarImpacted),
                                FollowUp = isformidExist.FollowUp ?? string.Empty,
                                Goal = isformidExist.Goal ?? string.Empty,
                                Plan = isformidExist.Plan ?? string.Empty,
                                Proposal = isformidExist.Proposal ?? string.Empty,
                                url = ConfigurationManager.AppSettings["AngularUrl"].ToString() + "/Main/studentevent?id=" + isformidExist.TrainingEventID
                            };
                            EmailWorkQueue emailWorkQueue = new EmailWorkQueue()
                            {
                                WorkItemType = "SendA3FormUpdateNotifyEmail",
                                KeyID = "",
                                KeyType = "",
                                SendToEmployee = Guid.Empty,
                                Subject = "A3 Tool form updated",
                                Body = "",
                                Template = "SendA3FormUpdateNotifyEmail.html",
                                TemplateContent = new JavaScriptSerializer().Serialize(sendA3FormUpdateNotifyEmail),
                                Status = "Pending",
                                CreatedDate = DateTime.UtcNow,
                                ModifiedDate = DateTime.UtcNow
                            };
                            await _emailWorkQueueService.Save(emailWorkQueue);
                        }
                            //SendMail.SendA3FormUpdateNotifyEmail(eventCreator, isformidExist, input.mentorEmail, input.mentorName);

                        return new ResponseViewModel { Code = 200, Message = "Records Successfully saved!" };
                    }
                    else
                    {
                        return new ResponseViewModel { Code = 403, Message = "Records not found" };
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<TrainingEventA3Diagram> GetA3FormDataById(string eventid)
        {
            var formdata = await _context.TrainingEventA3Diagram.FirstOrDefaultAsync(x => x.TrainingEventID == new Guid(eventid));
            return formdata;
        }

        public async Task<ResponseViewModel> ApproveEventbyMentorFromEventId(string eventid)
        {
            try
            {
                var iseventExist = await _context.TrainingEvents.FindAsync(new Guid(eventid));
                var compsetting = await _context.CompanySettings.Where(x => x.CompanyId == iseventExist.CompanyID).FirstOrDefaultAsync();
                Employee EmployeeDetail = new Employee();
                if (compsetting != null)
                {
                    EmployeeDetail = await _context.Employees.Where(x => x.ID == compsetting.A3DollarApprover).FirstOrDefaultAsync();
                }
                if (iseventExist != null)
                {
                    iseventExist.Status = "Pending Approval";
                    _context.SaveChanges();
                    if (EmployeeDetail != null)
                    {

                        EmailForDollarApproveViewModel emailForDollarApproveViewModel = new EmailForDollarApproveViewModel()
                        {
                            EmployeeEmail = EmployeeDetail.Email,
                            EmployeeName = EmployeeDetail.FirstName + " " + EmployeeDetail.LastName,
                            TrainingEventName = iseventExist.Name,
                            TrainingEventId = eventid,
                            AngularURL = ConfigurationManager.AppSettings["AngularUrl"].ToString()+ "Main/studentevent?id=" + eventid,
                        };
                        EmailWorkQueue emailWorkQueue = new EmailWorkQueue()
                        {
                            WorkItemType = "EmailForDollarApprove",
                            KeyID = eventid,
                            KeyType = "TrainingEvent",
                            SendToEmployee = Guid.Empty,
                            Subject = "Event ready for financial approval - " + emailForDollarApproveViewModel.TrainingEventName,
                            Body = "",
                            Template = "EmailForDollarApprove.html",
                            TemplateContent = new JavaScriptSerializer().Serialize(emailForDollarApproveViewModel),
                            Status = "Pending",
                            CreatedDate = DateTime.UtcNow,
                            ModifiedDate = DateTime.UtcNow
                        };
                        await _emailWorkQueueService.Save(emailWorkQueue);
                        //SendMail.SendEmailforDollarApprove(EmployeeDetail, iseventExist);

                    }
                    return new ResponseViewModel { Code = 200, Message = "Event Successfully Approved" };
                
                }
                else
                {
                    return new ResponseViewModel { Code = 403, Message = "Event not found" };
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<ResponseViewModel> ApproveEventbyDollarApproverFromEventId(string eventid)
        {
            try
            {
                var iseventExist = await _context.TrainingEvents.FindAsync(new Guid(eventid));
                var compsetting = await _context.CompanySettings.Where(x => x.CompanyId == iseventExist.CompanyID).FirstOrDefaultAsync();
                Employee EmployeeDetail = new Employee();
                if (compsetting != null)
                {
                    EmployeeDetail = await _context.Employees.Where(x => x.ID == compsetting.A3DollarApprover).FirstOrDefaultAsync();
                }
                if (iseventExist != null)
                {
                    iseventExist.Status = "Closed";
                    iseventExist.ClosedDate = DateTime.UtcNow;
                    iseventExist.ClosedBy = EmployeeDetail.ID;
                    iseventExist.Isclosed = true;
                    _context.SaveChanges();
                    await SendCloseEventMail(eventid, iseventExist, false);
                    return new ResponseViewModel { Code = 200, Message = "Event Successfully Approved" };
                }
                else
                {
                    return new ResponseViewModel { Code = 403, Message = "Event not found" };
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public async Task<List<A3TrainingEventViewModel>> GetA3TrainingEventsByCompanyID(string companyID, bool isActive)
        {
            try
            {
                Guid CompanyID = new Guid(companyID);
                List<A3TrainingEventViewModel> a3TraingEevent = await _context.Database.SqlQuery<A3TrainingEventViewModel>(
                    "dbo.usp_GetA3TrainingEventsByCompanyID @CompanyID = @companyID, @IsActive = @isActive",
                    new SqlParameter("companyID", CompanyID),
                    new SqlParameter("isActive", isActive)).ToListAsync();

                return a3TraingEevent;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<KaizenBoardViewModel>> GetKaizenBoard(string companyID)
        {
            try
            {
                Guid CompanyID = new Guid(companyID);
                List<KaizenBoardViewModel> kaizenBoard = await _context.Database.SqlQuery<KaizenBoardViewModel>(
                    "dbo.usp_GetKaizenBoard @CompanyID = @companyID",
                    new SqlParameter("companyID", CompanyID)).ToListAsync();
                List<KaizenBoardViewModel> kaizenBoardList = new List<KaizenBoardViewModel>();

                for (int m = 0; m < kaizenBoard.Count; m++)
                {
                    List<KaizenBoardViewModel> result = kaizenBoard.Where(d => d.id == kaizenBoard[m].id).ToList();
                    KaizenBoardViewModel data = new KaizenBoardViewModel();
                    List<Intials> inti = new List<Intials>();
                    string teams = "";
                    for (int i = 0; i < result.Count; i++)
                    {

                        teams = teams + "," + result[i].Intials;
                        data.Intials = teams;
                        data.id = result[i].id;
                        data.Idea = result[i].Idea;
                        data.Leader = result[i].Leader;
                        data.Notes = result[i].Notes;
                        data.Plan = result[i].Plan;
                        data.StartDate = result[i].StartDate;
                        data.TrainingEventFormat = result[i].TrainingEventFormat;
                        data.TrainingEventName = result[i].TrainingEventName;
                        data.DollarImpacted = result[i].DollarImpacted;
                        data.Do = result[i].Do;
                        data.Complete = result[i].Complete;
                        data.Check = result[i].Check;
                        data.Attendee = result[i].Attendee;

                        if (!string.IsNullOrWhiteSpace(data.Attendee) && !string.IsNullOrWhiteSpace(result[i].Intials))
                        {
                            inti.Add(new Intials
                            {
                                FullName = data.Attendee,
                                ShortName = result[i].Intials
                            });
                        }
                    }

                    if (result.Count > 0)
                    {
                        if (!kaizenBoardList.Any(d => d.id == data.id))
                        {
                            data.Team = inti;
                            data.Intials = data.Intials.Substring(1, data.Intials.Length - 1);
                            kaizenBoardList.Add(data);
                        }

                    }



                }

                return kaizenBoardList.OrderByDescending(d => d.StartDate).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<List<A3TrainingEventCommunication>> GetA3TrainingEventsCommData(string eventid)
        {
            try
            {
                var isCommExist = await _context.A3TrainingEventCommunication.Where(x => x.TrainingEventID == new Guid(eventid)).ToListAsync();
                if (isCommExist.Count() > 0)
                {
                    return isCommExist;
                }
                return null;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public async Task<ResponseViewModel> SaveA3TrainingEventsCommData(string eventid, string userid, string message)
        {
            try
            {
                string senderfullname = string.Empty;
                string receiveremail = string.Empty;
                string receiverfullname = string.Empty;
                Guid receiverid = Guid.Empty;
                bool isMentor = false;
                
                Mentor eventMentor = await (from e in _context.Mentors
                                            join ud in _context.UserDetails on e.ID equals ud.EmployeeID
                                            where ud.ID == new Guid(userid)
                                            select e).FirstOrDefaultAsync();
                if (eventMentor != null)
                {
                    senderfullname = eventMentor.FirstName + " " + eventMentor.LastName;
                    Guid? useridfromevent = _context.TrainingEvents.Where(x => x.ID == new Guid(eventid)).FirstOrDefault().owner;
                    Employee eventUser = await (from e in _context.Employees
                                                join ud in _context.UserDetails on e.ID equals ud.EmployeeID
                                                where ud.ID == useridfromevent
                                                select e).FirstOrDefaultAsync();
                    receiveremail = eventUser.Email;
                    receiverid = eventUser.ID;
                    receiverfullname = eventUser.FirstName + " " + eventUser.LastName;
                    isMentor = true;
                }
                   
                else
                {
                    Employee eventUser = await (from e in _context.Employees
                                                join ud in _context.UserDetails on e.ID equals ud.EmployeeID
                                                where ud.ID == new Guid(userid)
                                                select e).FirstOrDefaultAsync();
                    if (eventUser != null)
                    {
                        senderfullname = eventUser.FirstName + " " + eventUser.LastName;
                        var mentorinEmpTable = await _context.Employees.FindAsync(eventUser.MentorId);
                        if (mentorinEmpTable != null)
                        {
                            receiveremail = mentorinEmpTable.Email;
                            receiverid = mentorinEmpTable.ID;
                            receiverfullname = mentorinEmpTable.FirstName + " " + mentorinEmpTable.LastName;
                        }
                        else
                        {
                            var mentorinMentorTable = await _context.Mentors.FindAsync(eventUser.MentorId);
                            receiveremail = mentorinMentorTable.Email;
                            receiverid = mentorinMentorTable.ID;
                            receiverfullname = mentorinMentorTable.FirstName + " " + mentorinMentorTable.LastName;
                        }
                    }
                        
                }


                if (!string.IsNullOrEmpty(eventid))
                {
                    var trainingformatid =  _context.TrainingEvents.Where(x => x.ID == new Guid(eventid)).FirstOrDefault().TrainingEventFormatID.ToString(); 


                    A3TrainingEventCommunication communication = new A3TrainingEventCommunication
                    {
                        TrainingEventID = new Guid(eventid),
                        Message = message,
                        SentBy = new Guid(userid),
                        SentDate = DateTime.UtcNow,
                        MessageSender = senderfullname
                    };
                    _context.A3TrainingEventCommunication.Add(communication);
                    _context.SaveChanges();
                    A3_KaizenCommunicationViewModel a3_kaizenCommunicationViewModel = new A3_KaizenCommunicationViewModel()
                    {
                        receiveremail = receiveremail,
                        Username = receiverfullname,
                        senderfullname = senderfullname,
                        usermessage = message,
                        URL = isMentor  ? ConfigurationManager.AppSettings["AngularUrl"].ToString() + "/Main/create-event?id=" + eventid : ConfigurationManager.AppSettings["AngularUrl"].ToString() + "/Main/studentevent?id=" + eventid
                    };
                    EmailWorkQueue emailWorkQueue = new EmailWorkQueue()
                    {
                        WorkItemType = "A3Communication",
                        KeyID = Convert.ToString(eventid),
                        KeyType = "",
                        SendToEmployee = receiverid,
                        Subject = trainingformatid.Equals("5518993A-EFC0-4AD0-BCD7-BEAEA42CC2CE", StringComparison.InvariantCultureIgnoreCase) ? "Kaizen Communication Notification" : "A3 Communication Notification",
                        Body = "",
                        Template = "A3Communication.html",
                        TemplateContent = new JavaScriptSerializer().Serialize(a3_kaizenCommunicationViewModel),
                        Status = "Pending",
                        CreatedDate = DateTime.UtcNow,
                        ModifiedDate = DateTime.UtcNow
                    };
                    await _emailWorkQueueService.Save(emailWorkQueue);
                    //SendMail.SendEmailforA3Communication(receiveremail, eventsid, trainingformatid.Equals("5518993A-EFC0-4AD0-BCD7-BEAEA42CC2CE", StringComparison.InvariantCultureIgnoreCase) ? "Kaizen Communication Notification": "A3 Communication Notification", receiverfullname, senderfullname, message, isMentor) ;
                    return new ResponseViewModel { Code = 200, Message = "Mesage successfully saved" };
                }
                else
                {
                    return new ResponseViewModel { Code = 403, Message = "event not found" };
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Kaizen tools service created by Saurabh (2/08/2021)------

        public async Task<ResponseViewModel> SaveKaizenFormFields(KaizenFormViewModel input)
        {
            try
            {

                Employee eventCreator = await (from e in _context.Employees
                                               join ud in _context.UserDetails on e.ID equals ud.EmployeeID
                                               where ud.ID == new Guid(input.UserID)
                                               select e).FirstOrDefaultAsync();
                if (input.id == null)
                {
                    TrainingEventKaizenDiagram field = new TrainingEventKaizenDiagram
                    {
                        TrainingEventID = new Guid(input.TrainingEventID),
                        DefineTheProblem = input.DefineTheProblem ?? "",
                        CurrentCondition = input.CurrentCondition ?? "",
                        Analysis = input.Analysis ?? "",
                        IsActive = true,
                        FollowUp = input.FollowUp ?? "",
                        Goal = input.Goal ?? "",
                        ImplementationPlan = input.ImplementationPlan ?? "",
                        ActionItemTimeline = input.ActionItemTimeline ?? "",
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = new Guid(input.UserID),
                        DollarImpacted = input.DollarImpacted,
                        IsDollarValueApproved = false,
                        DollarValueApprovedBy = Guid.Empty,
                        DollarValueApprovedDate = DateTime.UtcNow,
                        AssignedTo = new Guid(input.AssignedTo)
                    };
                    _context.TrainingEventKaizenDiagrams.Add(field);
                    await _context.SaveChangesAsync();

                    //TrainingEventAttendee field2 = new TrainingEventAttendee
                    //{
                    //    TrainigEventID = new Guid(input.TrainingEventID),
                    //    EmployeeID = new Guid(input.AssignedTo),
                    //    Time = 0,
                    //    Test = 0
                    //};
                    //_context.TrainingEventAttendees.Add(field2);
                    //await _context.SaveChangesAsync();

                    if (eventCreator != null && !string.IsNullOrWhiteSpace(input.mentorEmail))
                    {
                        SendKaizenFormUpdateNotifyEmail sendKaizenFormUpdateNotifyEmail = new SendKaizenFormUpdateNotifyEmail()
                        {
                            receiverEmail = input.mentorEmail,
                            receivername = input.mentorName,
                            FirstName = eventCreator.FirstName,
                            LastName = eventCreator.LastName,
                            Email = eventCreator.Email,
                            Analyses = field.Analysis ?? string.Empty,
                            DefineTheProblem = field.DefineTheProblem ?? string.Empty,
                            Condition = field.CurrentCondition ?? string.Empty,
                            DollarImpacted = Convert.ToString(field.DollarImpacted),
                            FollowUp = field.FollowUp ?? string.Empty,
                            Goal = field.Goal ?? string.Empty,
                            Plan = field.ImplementationPlan ?? string.Empty,
                            ActionItemTimeline = field.ActionItemTimeline ?? string.Empty,
                            url = ConfigurationManager.AppSettings["AngularUrl"].ToString() + "/Main/studentevent?id=" + field.TrainingEventID
                        };
                        EmailWorkQueue emailWorkQueue = new EmailWorkQueue()
                        {
                            WorkItemType = "SendKaizenFormUpdateNotifyEmail",
                            KeyID = "",
                            KeyType = "",
                            SendToEmployee = Guid.Empty,
                            Subject = "Kaizen Tool form updated",
                            Body = "",
                            Template = "SendKaizenFormUpdateNotifyEmail.html",
                            TemplateContent = new JavaScriptSerializer().Serialize(sendKaizenFormUpdateNotifyEmail),
                            Status = "Pending",
                            CreatedDate = DateTime.UtcNow,
                            ModifiedDate = DateTime.UtcNow
                        };
                        await _emailWorkQueueService.Save(emailWorkQueue);
                    }
                        //SendMail.SendKaizenFormUpdateNotifyEmail(eventCreator, field, input.mentorEmail, input.mentorName);


                    return new ResponseViewModel { Code = 200, Message = "Records Successfully saved!" };
                }
                else
                {
                    var isformidExist = await _context.TrainingEventKaizenDiagrams.FindAsync(input.id);
                    if (isformidExist != null)
                    {
                        isformidExist.TrainingEventID = new Guid(input.TrainingEventID);
                        isformidExist.DefineTheProblem = input.DefineTheProblem ?? "";
                        isformidExist.CurrentCondition = input.CurrentCondition ?? "";
                        isformidExist.Analysis = input.Analysis ?? "";
                        isformidExist.IsActive = true;
                        isformidExist.FollowUp = input.FollowUp ?? "";
                        isformidExist.Goal = input.Goal ?? "";
                        isformidExist.ImplementationPlan = input.ImplementationPlan ?? "";
                        isformidExist.ActionItemTimeline = input.ActionItemTimeline ?? "";
                        isformidExist.Modifieddate = DateTime.UtcNow;
                        isformidExist.ModifiedBy = new Guid(input.UserID);
                        isformidExist.DollarImpacted = input.DollarImpacted;
                        isformidExist.IsDollarValueApproved = false;
                        isformidExist.DollarValueApprovedBy = Guid.Empty;
                        isformidExist.DollarValueApprovedDate = DateTime.UtcNow;
                        isformidExist.AssignedTo = new Guid(input.AssignedTo);


                        await _context.SaveChangesAsync();
                        if (eventCreator != null && !string.IsNullOrWhiteSpace(input.mentorEmail))
                        {
                            SendKaizenFormUpdateNotifyEmail sendKaizenFormUpdateNotifyEmail = new SendKaizenFormUpdateNotifyEmail()
                            {
                                receiverEmail = input.mentorEmail,
                                receivername = input.mentorName,
                                FirstName = eventCreator.FirstName,
                                LastName = eventCreator.LastName,
                                Email = eventCreator.Email,
                                Analyses = isformidExist.Analysis ?? string.Empty,
                                DefineTheProblem = isformidExist.DefineTheProblem ?? string.Empty,
                                Condition = isformidExist.CurrentCondition ?? string.Empty,
                                DollarImpacted = Convert.ToString(isformidExist.DollarImpacted),
                                FollowUp = isformidExist.FollowUp ?? string.Empty,
                                Goal = isformidExist.Goal ?? string.Empty,
                                ActionItemTimeline = isformidExist.ActionItemTimeline ?? string.Empty,
                                 Plan= isformidExist.ImplementationPlan ?? string.Empty,
                                url = ConfigurationManager.AppSettings["AngularUrl"].ToString() + "/Main/studentevent?id=" + isformidExist.TrainingEventID
                            };
                            EmailWorkQueue emailWorkQueue = new EmailWorkQueue()
                            {
                                WorkItemType = "SendKaizenFormUpdateNotifyEmail",
                                KeyID = "",
                                KeyType = "",
                                SendToEmployee = Guid.Empty,
                                Subject = "Kaizen Tool form updated",
                                Body = "",
                                Template = "SendKaizenFormUpdateNotifyEmail.html",
                                TemplateContent = new JavaScriptSerializer().Serialize(sendKaizenFormUpdateNotifyEmail),
                                Status = "Pending",
                                CreatedDate = DateTime.UtcNow,
                                ModifiedDate = DateTime.UtcNow
                            };
                            await _emailWorkQueueService.Save(emailWorkQueue);
                        }
                            //SendMail.SendKaizenFormUpdateNotifyEmail(eventCreator, isformidExist, input.mentorEmail, input.mentorName);

                        return new ResponseViewModel { Code = 200, Message = "Records Successfully saved!" };
                    }
                    else
                    {
                        return new ResponseViewModel { Code = 403, Message = "Records not found" };
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<TrainingEventKaizenDiagram> GetKaizenFormDataById(string eventid)
        {
            var formdata = await _context.TrainingEventKaizenDiagrams.FirstOrDefaultAsync(x => x.TrainingEventID == new Guid(eventid));
            return formdata;
        }

        public async Task<List<KaizenTrainingEventViewModel>> GetKaizenTrainingEventsByCompanyID(string companyID, bool isActive)
        {
            try
            {
                Guid CompanyID = new Guid(companyID);
                List<KaizenTrainingEventViewModel> kaizenTraingEevent = await _context.Database.SqlQuery<KaizenTrainingEventViewModel>(
                    "dbo.usp_GetKaizenTrainingEventsByCompanyID @CompanyID = @companyID, @IsActive = @isActive",
                    new SqlParameter("companyID", CompanyID),
                    new SqlParameter("isActive", isActive)).ToListAsync();

                return kaizenTraingEevent;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static StringBuilder GetEmpTable(List<Employee> emp)
        {
            StringBuilder orderStr = new StringBuilder();

            orderStr.Append("<table style=\"font-size: 10pt; font-family: Verdana, Arial, Helvetica, sans-serif; border: 1px solid #ccc; text-align: left;\" cellspacing=\"0\" cellpadding=\"10\">");
            orderStr.Append("<tr><th style=\"text-align:left\">Employee code</th><th style=\"text-align:left\">Employee Name</th><th style=\"text-align:left\">Email</th></tr>");

            foreach (var item in emp)
            {
                orderStr.Append("<tr>" +
               "<td style=\"text-align:left\">" + item.EmployeeCode + "</td>" +
               "<td style=\"text-align:left\">" + item.FirstName + " " + item.LastName + "</td>" +
               "<td style=\"text-align:left\">" + (item.Email) + "</td>" +

                "</tr>");

            }

            orderStr.Append("</table>");

            return orderStr;
        }
    }
}
