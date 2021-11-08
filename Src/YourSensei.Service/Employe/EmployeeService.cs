using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using YourSensei.Data;
using YourSensei.Service;
using YourSensei.Utility;
using YourSensei.ViewModel;

namespace YourSensei.Service
{
    public class EmployeeService : IEmployeeService
    {
        private readonly YourSensei_DBEntities _context;
        private readonly IMentorService _mentorService;
        private readonly ICreditLogService _creditLogService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IEmailWorkQueueService _emailWorkQueueService;
        public EmployeeService(YourSensei_DBEntities context, MentorService mentorService, CreditLogService creditLogService, SubscriptionService subscriptionService, EmailWorkQueueService emailWorkQueueService)
        {
            _context = context;
            _mentorService = mentorService;
            _creditLogService = creditLogService;
            _subscriptionService = subscriptionService;
            _emailWorkQueueService = emailWorkQueueService;
        }

        public async Task<List<EmployeeResponseViewModel>> GetAllEmployee(string companyid)
        {
            try
            {
                List<EmployeeResponseViewModel> listofemp = new List<EmployeeResponseViewModel>();
                var empdata = string.IsNullOrEmpty(companyid) == false ?
                    await _context.Employees.Where(a => a.CompanyId == new Guid(companyid)).OrderBy(n => n.FirstName).ToListAsync()
                    : await _context.Employees.OrderBy(n => n.FirstName).ToListAsync();

                foreach (var a in empdata)
                {
                    var userDetailID = await _context.UserDetails.Where(r => r.EmployeeID == a.ID).Select(o => o.ID).FirstOrDefaultAsync();
                    var ListOflogs = await Task.Run(() => _context.usp_GetCreditLogsByUserDetailID(userDetailID, a.IsActive).OrderBy(n => n.FirstName).ToList());

                    string mentorName = "";
                    if (a.IsExternalMentor)
                    {
                        mentorName = (from m in _context.Mentors
                                      where m.IsActive == true && m.ID == a.MentorId
                                      select m.FirstName + " " + m.LastName).SingleOrDefault();

                        if (_context.MentorCompanyMappings.Any(m => m.MentorID == a.MentorId))
                            mentorName = mentorName + " (e)";
                        else
                            mentorName = mentorName + " (g)";
                    }
                    else
                    {
                        mentorName = (from e in _context.Employees
                                      where e.IsActive == true && e.ID == a.MentorId
                                      select e.FirstName + " " + e.LastName).SingleOrDefault();
                    }

                    UserDetail userDetail = _context.UserDetails.Where(x => x.EmployeeID == a.ID).FirstOrDefault();
                    EmployeeResponseViewModel obj = new EmployeeResponseViewModel
                    {
                        Id = a.ID.ToString(),
                        Email = a.Email,
                        userDetialID = Convert.ToString(userDetail.ID.ToString()) ?? "",
                        FirstName = a.FirstName,
                        CompanyId = Convert.ToString(a.CompanyId),
                        LastName = a.LastName,
                        RoleId = _context.UserRoles.Where(b => b.ID == a.UserRoleID).Select(b => b.Name == "Other" ? b.Name + " (" + a.OtherRole + ")" : b.Name).FirstOrDefault(),
                        MentorId = mentorName,
                        IsMentor = a.IsMentor,
                        IsActive = a.IsActive,
                        EmployeeCode = a.EmployeeCode,
                        Credit = ListOflogs.Sum(item => item.Credit) ,
                        UserCategory = userDetail.UserType.ToString() ?? "" 
                       
                    };

                    if (obj.RoleId == "N/A")
                    {
                        Employee employee = _context.Employees.Where(x => x.EmployeeCode == obj.EmployeeCode).FirstOrDefault();
                        obj.RoleId = employee.OtherRole;
                    }

                    listofemp.Add(obj);
                }
                return listofemp;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ResponseViewModel> AddEmployee(EmployeeResponseViewModel emp)
        {
            try
            {
                var subsDetail =  await _subscriptionService.GetSubscribedPlan(new Guid(emp.CompanyId), Guid.Empty);
                int empdata =  await _context.Employees.Where(a => a.CompanyId == new Guid(emp.CompanyId) && a.IsActive == true).CountAsync();
                if (empdata < subsDetail.NumberOfEmployees)
                {
                    var result = _context.UserDetails.Where(d => d.UserName == emp.Email).ToList();
                    if (result.Count == 0)
                    {
                        Member member = new Member();
                        member.LastName = emp.LastName;
                        member.Email = emp.Email;
                        member.FirstName = emp.FirstName;
                        member.HomePhone = "";
                        member.WorkPhone = "";
                        member.AddressLine1 = "";
                        member.AddressLine2 = "";
                        member.City = "";
                        member.State = "";
                        member.ZipCode = "";
                        member.Country = "";
                        member.IsActive = true;
                        member.CreatedBy = new Guid();
                        member.ModifiedBy = new Guid();
                        member.CreatedDate = DateTime.UtcNow;
                        member.ModifiedDate = DateTime.UtcNow;
                        _context.Members.Add(member);
                        int rowsAffected = await _context.SaveChangesAsync();

                        if (rowsAffected > 0)
                        {
                            rowsAffected = 0;
                            Employee objemp = new Employee();

                            var defaultpassword = ConfigurationManager.AppSettings["DefaultPass"].ToString();
                            string hashpass = PasswordHash.CreateHash(defaultpassword);
                            objemp.MemberID = member.MemberID;
                            objemp.FirstName = emp.FirstName;
                            objemp.LastName = emp.LastName;
                            objemp.IsMentor = emp.IsMentor;
                            if (emp.MentorId != "" && emp.MentorId != null)
                            {
                                objemp.MentorId = new Guid(emp.MentorId);
                                objemp.IsExternalMentor = await _mentorService.IsExists(emp.MentorId);
                            }

                            objemp.CompanyId = new Guid(emp.CompanyId);
                            objemp.UserRoleID = new Guid(emp.RoleId);
                            objemp.OtherRole = emp.OtherRole;
                            objemp.EmployeeCode = emp.EmployeeCode;
                            objemp.CreditScore = emp.Credit;
                            objemp.Email = emp.Email;
                            objemp.CreatedBy = new Guid(emp.CreatedBy);
                            objemp.CreatedDate = DateTime.UtcNow;
                            objemp.ModifiedBy = new Guid(emp.ModifiedBy);
                            objemp.ModifiedDate = DateTime.UtcNow;
                            objemp.IsActive = true;
                            objemp.ID = Guid.NewGuid();
                            _context.Employees.Add(objemp);
                            rowsAffected = _context.SaveChanges();

                            if (rowsAffected > 0)
                            {
                                UserDetail user = new UserDetail();
                                user.IsActive = true;
                                user.CreatedDate = DateTime.UtcNow;
                                user.ModifiedDate = System.DateTime.UtcNow;
                                user.CreatedBy = new Guid(emp.CreatedBy);
                                user.ModifiedBy = new Guid(emp.CreatedBy);
                                user.EmployeeID = objemp.ID;
                                user.Password = hashpass;
                                user.UserName = emp.Email;
                                user.IsApproved = true;
                                user.IsRejected = false;
                                user.IsInitialPassword = true;
                                user.ID = Guid.NewGuid();
                                user.UserType = new Guid(emp.UserCategory);
                                user.RequestDate = DateTime.UtcNow;
                                user.ApprovalDate = DateTime.UtcNow;
                                user.RejectedDate = null;
                                user.IsInitialPassword = true;
                                _context.UserDetails.Add(user);
                                _context.SaveChanges();

                                Data.CreditLog creditLog = new Data.CreditLog();
                                creditLog.ID = Guid.NewGuid();
                                creditLog.KeyType = "EmployeeInitialCredit";
                                creditLog.KeyID = objemp.ID;
                                creditLog.MemberID = member.MemberID;
                                creditLog.UserDetailID = user.ID;
                                creditLog.FirstName = emp.FirstName;
                                creditLog.LastName = emp.LastName;
                                creditLog.CompanyID = new Guid(emp.CompanyId);
                                creditLog.AwardedDate = DateTime.UtcNow;
                                creditLog.Credit = emp.Credit;
                                _context.CreditLogs.Add(creditLog);
                                _context.SaveChanges();

                                //SendMail.SendWelcomeEmailToEmployee(objemp);
                                WelMailToEmpViewModel welMailToEmp = new WelMailToEmpViewModel()
                                {
                                    Username = objemp.FirstName + " " + objemp.LastName,
                                    UserEmail = objemp.Email
                                };
                                EmailWorkQueue userEmailWorkQueue = new EmailWorkQueue
                                {
                                    WorkItemType = "SendWelcomeEmailToEmployee",
                                    KeyID = "",
                                    KeyType = "",
                                    SendToEmployee = Guid.Empty,
                                    Subject = "Welcome to Your Sensei",
                                    Body = "",
                                    Template = "SendWelcomeEmailToEmployee.html",
                                    TemplateContent = new JavaScriptSerializer().Serialize(welMailToEmp),
                                    Status = "Pending",
                                    CreatedDate = DateTime.UtcNow,
                                    ModifiedDate = DateTime.UtcNow
                                };
                                await _emailWorkQueueService.Save(userEmailWorkQueue);
                            }
                        }

                        return new ResponseViewModel { Code = 200, Message = "Employee has been created successfully!" };
                    }
                    else
                    {
                        return new ResponseViewModel { Code = 403, Message = "Email already exist!" };
                    }
                }
                else
                    return new ResponseViewModel { Code = 400, Message = "You Don't add more employee. !" };

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<RoleResponseViewModel>> GetAllRole()
        {
            try
            {
                var data = await _context.UserRoles.Where(a => a.IsActive == true).Select(a => new RoleResponseViewModel
                {
                    RoleId = a.ID,
                    RoleName = a.Name,
                    IsActive = a.IsActive
                }).ToListAsync();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<EmployeeResponseViewModel>> GetAllMentor(string companyid)
        {
            try
            {
                var data = await _context.Employees.Where(a => a.IsMentor == true &&
                    a.IsActive == true &&
                    a.CompanyId == new Guid(companyid))
                    .Select(a => new EmployeeResponseViewModel
                    {
                        Id = a.ID.ToString(),
                        MentorId = a.MentorId.ToString(),
                        FirstName = a.FirstName,
                        LastName = a.LastName
                    }).ToListAsync();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ResponseViewModel> DeleteEmployee(string employeeid)
        {
            try
            {
                var data = await _context.Employees.Where(a => a.ID == new Guid(employeeid)).FirstOrDefaultAsync();
                if (data != null)
                {
                    UserDetail userDetail = _context.UserDetails.Where(a => a.EmployeeID == data.ID).FirstOrDefault();
                    if (userDetail != null)
                    {
                        userDetail.IsActive = false;
                        _context.SaveChanges();
                    }

                    Member member = _context.Members.Where(a => a.MemberID == data.MemberID).FirstOrDefault();
                    if (member != null)
                    {
                        member.IsActive = false;
                        _context.SaveChanges();
                    }

                    data.IsActive = false;
                    _context.SaveChanges();
                }
                return new ResponseViewModel { Code = 200, Message = "Your Employee record has been deleted successfully!" };

            }
            catch (Exception)
            {
                return new ResponseViewModel { Code = 400, Message = "Something went wrong please try again after a moment!" };
            }
        }

        public async Task<EmployeeResponseViewModel> GetEmployeeById(string empid)
        {
            try
            {
                var data = await (from e in _context.Employees
                                  join ud in _context.UserDetails on e.ID equals ud.EmployeeID
                                  where e.ID == new Guid(empid)
                                  select new EmployeeResponseViewModel()
                {
                    Id = e.ID.ToString(),
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    RoleId = e.UserRoleID.ToString(),
                    MentorId = e.MentorId.ToString(),
                    IsMentor = e.IsMentor,
                    EmployeeCode = e.EmployeeCode,
                    Credit = e.CreditScore,
                    IsActive = e.IsActive,
                    Email = e.Email,
                    CompanyId = e.CompanyId.ToString(),
                    UserCategory = ud.UserType.ToString(),
                    OtherRole = e.OtherRole,
                    MemberID=e.MemberID
                }).FirstOrDefaultAsync();

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ResponseViewModel> UpdateEmployee(EmployeeResponseViewModel emp)
        {
            try
            {
                var data = await _context.Employees.Where(a => a.ID == new Guid(emp.Id)).Select(a => a).FirstOrDefaultAsync();
                if (data != null)
                {
                    data.FirstName = emp.FirstName;
                    data.LastName = emp.LastName;
                    data.UserRoleID = new Guid(emp.RoleId);
                    data.OtherRole = emp.OtherRole;
                    if (emp.MentorId != "" && emp.MentorId != null)
                    {
                        data.MentorId = new Guid(emp.MentorId);
                        data.IsExternalMentor = await _mentorService.IsExists(emp.MentorId);
                    }

                    data.IsMentor = emp.IsMentor;
                    data.EmployeeCode = emp.EmployeeCode;
                    data.CreditScore = emp.Credit;
                    data.IsActive = emp.IsActive;
                    data.Email = emp.Email;
                    data.ModifiedBy = new Guid(emp.ModifiedBy);
                    data.ModifiedDate = DateTime.UtcNow;
                    _context.SaveChanges();

                    UserDetail userDetail = _context.UserDetails.Where(x => x.EmployeeID == new Guid(emp.Id)).FirstOrDefault();
                    if (userDetail != null)
                    {
                        userDetail.UserType = new Guid(emp.UserCategory);
                        _context.SaveChanges();
                    }

                    return new ResponseViewModel { Code = 200, Message = "Employee has been successfully updated!" };
                }
                else
                {
                    return new ResponseViewModel { Code = 403, Message = "Employee not Found!" };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Employee> GetProfileByEmail(string email)
        {
            try
            {
                var empDetails = await _context.Employees.Where(a => a.Email == email).FirstOrDefaultAsync();
                return empDetails;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Mentor> GetMentorProfileByEmail(string email)
        {
            try
            {
                var empDetails = await _context.Mentors.Where(a => a.Email == email).FirstOrDefaultAsync();
                return empDetails;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<GetEmployeeByMentorViewModel>> GetEmployeeByMentorID(string mentorID)
        {
            try
            {
                List<GetEmployeeByMentorViewModel> res = new List<GetEmployeeByMentorViewModel>();
                var employees = await _context.Employees.Where(a => a.MentorId == new Guid(mentorID) && a.IsActive == true).ToListAsync();
                foreach (var item in employees)
                {
                    GetEmployeeByMentorViewModel mod = new GetEmployeeByMentorViewModel
                    {
                        ID = item.ID,
                        IsActive = item.IsActive,
                        CompanyId = item.CompanyId,
                        FirstName = item.FirstName,
                        LastName = item.LastName,
                        Email = item.Email,
                        EmployeeCode = item.EmployeeCode,
                        CreditScore = item.CreditScore,
                        Gender = item.Gender,
                        MentorId = item.MentorId,
                        UserRoleID = item.UserRoleID,
                        userId = _context.UserDetails.Where(x => x.EmployeeID == item.ID).FirstOrDefault().ID,
                        IsMentor = item.IsMentor
                    };
                    res.Add(mod);
                }
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Employee> GetEmployeeByUserDetailID(Guid userDetailID)
        {
            try
            {
                Employee employee = await (from ud in _context.UserDetails
                                           join e in _context.Employees on ud.EmployeeID equals e.ID
                                           where ud.ID == userDetailID
                                           select e).FirstOrDefaultAsync();

                return employee;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
