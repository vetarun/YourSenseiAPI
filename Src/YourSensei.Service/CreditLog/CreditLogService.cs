using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourSensei.Data;
using YourSensei.Utility;
using YourSensei.ViewModel;

namespace YourSensei.Service
{
    public class CreditLogService : ICreditLogService
    {
        private readonly YourSensei_DBEntities _context;

        public CreditLogService(YourSensei_DBEntities context)
        {
            _context = context;
        }

        public async Task<ResponseViewModel> SaveCreditLog(EmployeeResponseViewModel employee, decimal credit, string id, string logType)
        {
            try
            {
                Data.CreditLog creditLog = new Data.CreditLog()
                {
                    MemberID = employee.MemberID,
                    UserDetailID = new Guid(employee.userDetialID),
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    CompanyID = new Guid(employee.CompanyId),
                    AwardedDate = DateTime.UtcNow,
                    ID = Guid.NewGuid(),
                    Credit = credit,
                    KeyID = new Guid(id),
                    KeyType = logType
                };
                _context.CreditLogs.Add(creditLog);
                await _context.SaveChangesAsync();

                return new ResponseViewModel { Code = 200, Message = "Credit log has been created successfully!" };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<CreditLogByCompanyIdResponseViewModel>> GetCreditLogsByCompanyID(string companyID)
        {
            try
            {
                List<CreditLogByCompanyIdResponseViewModel> mod = new List<CreditLogByCompanyIdResponseViewModel>();
                
               
                var IsLogExist = await _context.CreditLogs.Where(x => x.CompanyID == new Guid(companyID) || new Guid(companyID) == new Guid("00000000-0000-0000-0000-000000000000")).OrderBy(x => x.AwardedDate).ToListAsync();
                IEnumerable<IGrouping<Guid, CreditLog>> creditGroup = IsLogExist.GroupBy(r => r.UserDetailID);
                //  var IsLogExist = await _context.CreditLogs.Where(x => x.CompanyID == new Guid(companyID) || new Guid(companyID) == new Guid("00000000-0000-0000-0000-000000000000")).ToListAsync();
                
                if (IsLogExist != null || IsLogExist.Count != 0)
                {
                    foreach (var log in creditGroup)
                    {
                        Mentor objmentor = new Mentor();
                        List<CreditLogResponseViewModel> loglist = new List<CreditLogResponseViewModel>();
                        List<CreditLog> creditList = log.ToList();
                        var EmpDetails = await _context.Employees.FirstOrDefaultAsync(x => x.ID == log.Key );
                        if (EmpDetails == null)
                        {
                            EmpDetails = await (from e in _context.Employees
                                                join u in _context.UserDetails on e.ID equals u.EmployeeID
                                                where u.ID == log.Key
                                                select e).FirstOrDefaultAsync();
                            if (EmpDetails == null)
                            {

                                objmentor = await (from m in _context.Mentors
                                                   join u in _context.UserDetails on m.ID equals u.EmployeeID
                                                   where u.ID == log.Key
                                                   select m).FirstOrDefaultAsync();
                            }
                        }
                        foreach (var item in creditList)
                        {
                           
                            TrainingEvent trainingEvent = await _context.TrainingEvents.Where(x => x.ID == item.KeyID).FirstOrDefaultAsync();
                            CompanyLibraryBook companyLibraryBook = await _context.CompanyLibraryBooks.Where(x => x.ID == item.KeyID).FirstOrDefaultAsync();

                            string eventName = "";
                            if (trainingEvent != null && item.KeyType == "TrainingEvent")
                                eventName = trainingEvent.Name;
                            else if (companyLibraryBook != null && item.KeyType == "Book")
                                eventName = "Book Read: " + companyLibraryBook.Title;
                            else if (item.KeyType == "EmployeeInitialCredit")
                                eventName = "Initial Credit";
                            if (EmpDetails != null || objmentor != null)
                            {
                                CreditLogResponseViewModel logs = new CreditLogResponseViewModel
                                {
                                    MenberID = item.MemberID,
                                    EmpCode = EmpDetails == null ? "" : EmpDetails.EmployeeCode,
                                    FirstName = EmpDetails == null ? objmentor.FirstName : EmpDetails.FirstName,
                                    LastName = EmpDetails == null ? objmentor.LastName : EmpDetails.LastName,
                                    CompanyName = item.CompanyID == new Guid("00000000-0000-0000-0000-000000000000") ? "" : _context.CompanyDetails.Find(item.CompanyID).companyname,
                                    Description = item.Description,
                                    AwardedDate = item.AwardedDate,
                                    Credit = item.Credit,
                                    Event = eventName
                                };
                                loglist.Add(logs);
                            }
                        }

                        loglist = loglist.OrderByDescending(a => a.AwardedDate).ToList();
                        CreditLogByCompanyIdResponseViewModel obj = new CreditLogByCompanyIdResponseViewModel
                        {
                           
                            EmpCode = EmpDetails == null ? "" : EmpDetails.EmployeeCode,
                            FirstName = EmpDetails == null ? objmentor.FirstName : EmpDetails.FirstName,
                            LastName = EmpDetails == null ? objmentor.LastName : EmpDetails.LastName,
                            SumOfLogs = loglist.Sum(x=>x.Credit),
                           
                            ListOfLogsForuser = loglist
                        };
                        mod.Add(obj);

                    }

                }

               
                return mod;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<CreditLogResponseViewModel>> GetAllEmployeewithMentor(string userid)
        {
            try
            {
                List<CreditLogResponseViewModel> data = new List<CreditLogResponseViewModel>();
                var employeedetails = _context.UserDetails.Where(a => a.ID == new Guid(userid)).Select(t => t).FirstOrDefault();
                var admindata = await _context.UserDetails.Where(a => a.EmployeeID == employeedetails.EmployeeID).Select(a => a.UserType).FirstOrDefaultAsync();
                string userTypeDescription = EnumHelper.GetDescription(Utility.UserCategory.CompanyAdmin);
                Guid companyAdminuserTypeID = _context.UserCategories.Where(a => a.Description == userTypeDescription).Select(d => d.Id).SingleOrDefault();
                if (admindata == companyAdminuserTypeID)
                {
                    var companyid = await _context.Employees.Where(a => a.ID == employeedetails.EmployeeID).Select(a => a.CompanyId).FirstOrDefaultAsync();
                    var empData = await _context.Employees.Where(a => a.CompanyId == companyid).ToListAsync();
                    foreach (var a in empData)
                    {
                        CreditLogResponseViewModel obj = new CreditLogResponseViewModel
                        {
                            FirstName = a.FirstName,
                            LastName = a.LastName,
                            EmpID = a.ID,
                            UserID = _context.UserDetails.Where(b => b.EmployeeID == a.ID).Select(t => t.ID).FirstOrDefault()
                    };
                        data.Add(obj);
                    }
                }
                else
                {
                    var empData = await _context.Employees.Where(a => a.MentorId == employeedetails.EmployeeID).ToListAsync();
                    foreach (var a in empData)
                    {
                        CreditLogResponseViewModel obj = new CreditLogResponseViewModel
                        {
                            FirstName = a.FirstName,
                            LastName = a.LastName,
                            EmpID = a.ID,
                            UserID = _context.UserDetails.Where(b => b.EmployeeID == a.ID).Select(t => t.ID).FirstOrDefault()
                        };
                        data.Add(obj);
                    }
                   
                }
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<CreditLogResponseViewModel>> GetCreditLogsByUserID(string userid, string companyID)
        {
            try
            {
                List<CreditLogResponseViewModel> loglist = new List<CreditLogResponseViewModel>();
                List<Data.CreditLog> IsLogExist;
                if (userid == "All")
                {
                    IsLogExist = await _context.CreditLogs.Where(x => x.CompanyID == new Guid(companyID)).OrderByDescending(a => a.KeyType == "EmployeeInitialCredit").ToListAsync();
                }
                else
                {
                    IsLogExist = await _context.CreditLogs.Where(x => x.UserDetailID == new Guid(userid)).OrderByDescending(a => a.KeyType == "EmployeeInitialCredit").ToListAsync();
                }
                if (IsLogExist != null || IsLogExist.Count != 0)
                {
                    foreach (var item in IsLogExist)
                    {
                        var EmpDetails = await _context.Employees.FirstOrDefaultAsync(x => x.ID == item.UserDetailID);

                        TrainingEvent trainingEvent = await _context.TrainingEvents.Where(x => x.ID == item.KeyID).FirstOrDefaultAsync();
                        CompanyLibraryBook companyLibraryBook = await _context.CompanyLibraryBooks.Where(x => x.ID == item.KeyID).FirstOrDefaultAsync();
                        string eventName = "";
                        if (trainingEvent != null && item.KeyType == "TrainingEvent")
                            eventName = trainingEvent.Name;
                        else if (companyLibraryBook != null && item.KeyType == "Book")
                            eventName = "Book Read: " + companyLibraryBook.Title;
                        else if (item.KeyType == "EmployeeInitialCredit")
                            eventName = "Initial Credit";
                        CreditLogResponseViewModel logs = new CreditLogResponseViewModel
                        {
                            MenberID = item.MemberID,
                            EmpCode = EmpDetails.EmployeeCode,
                            FirstName = EmpDetails.FirstName,
                            LastName = EmpDetails.LastName,
                            CompanyName = _context.CompanyDetails.Find(item.CompanyID).companyname,
                            Description = item.Description,
                            AwardedDate = item.AwardedDate,
                            Credit = item.Credit,
                            Event = eventName
                        };
                        loglist.Add(logs);
                    }
                }
                return loglist;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Boolean> IsMentorLoggedIn(string userid)
        {
            try
            {
                var useremployeeid = _context.UserDetails.Where(a => a.ID == new Guid(userid)).Select(a => a.EmployeeID).FirstOrDefault();
                var data = await _context.Employees.AnyAsync(a => a.ID == useremployeeid && a.IsMentor == true);
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<CreditLogByEmployeeIdResponseViewModel> GetCreditLogsByLoggedInUser(string userid, bool isActive)
        {
            try
            {
                CreditLogByEmployeeIdResponseViewModel mod = new CreditLogByEmployeeIdResponseViewModel();                
                mod.ListOflogs = await Task.Run(() => _context.usp_GetCreditLogsByUserDetailID(new Guid(userid), isActive).OrderByDescending(a => a.AwardedDate).ToList());
                mod.SumOfCredits = mod.ListOflogs.Sum(item => item.Credit);
                return mod;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
