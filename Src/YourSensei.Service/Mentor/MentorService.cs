using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using YourSensei.Data;
using YourSensei.Utility;
using YourSensei.ViewModel;

namespace YourSensei.Service
{
    public class MentorService : IMentorService
    {
        private readonly YourSensei_DBEntities _context;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IEmailWorkQueueService _emailWorkQueueService;
        public MentorService(YourSensei_DBEntities context, SubscriptionService subscriptionService, EmailWorkQueueService emailWorkQueueService)
        {
            _context = context;
            _subscriptionService = subscriptionService;
            _emailWorkQueueService = emailWorkQueueService;

        }

        public async Task<List<MentorResponseViewModel>> GetAllMentorsByCompanyID(string companyID)
        {
            try
            {
                List<MentorResponseViewModel> mentorResponseViewModels = new List<MentorResponseViewModel>();
                if (companyID != "00000000-0000-0000-0000-000000000000")
                {
                    mentorResponseViewModels = await (from m in _context.Mentors
                                                      join mcm in _context.MentorCompanyMappings on m.ID equals mcm.MentorID
                                                      where mcm.CompanyID == new Guid(companyID) && m.IsActive == true
                                                      select new MentorResponseViewModel()
                                                      {
                                                          Id = m.ID.ToString(),
                                                          FirstName = m.FirstName,
                                                          LastName = m.LastName,
                                                          IsActive = m.IsActive,
                                                          Phone = m.Phone,
                                                          Email = m.Email,
                                                          Gender = m.Gender,
                                                          CompanyId = mcm.CompanyID.ToString()
                                                      }).ToListAsync();
                }
                else
                {
                    string userTypeDescription = EnumHelper.GetDescription(Utility.UserCategory.GlobalMentor);
                    Guid userTypeID = _context.UserCategories.Where(a => a.Description == userTypeDescription).Select(d => d.Id).SingleOrDefault();
                    mentorResponseViewModels = await (from m in _context.Mentors
                                                      where m.IsActive == true && m.UserType == userTypeID
                                                      select new MentorResponseViewModel()
                                                      {
                                                          Id = m.ID.ToString(),
                                                          FirstName = m.FirstName,
                                                          LastName = m.LastName,
                                                          IsActive = m.IsActive,
                                                          Phone = m.Phone,
                                                          Email = m.Email,
                                                          Gender = m.Gender,
                                                          CompanyId = "00000000-0000-0000-0000-000000000000"
                                                      }).ToListAsync();
                }
                return mentorResponseViewModels;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ResponseViewModel> AddMentor(MentorResponseViewModel mentorResponseViewModel)
        {
            try
            {
                var subsDetail = await _subscriptionService.GetSubscribedPlan(new Guid(mentorResponseViewModel.CompanyId), Guid.Empty);
                int empdata = (from p in _context.MentorCompanyMappings
                               join s in _context.Mentors on p.MentorID equals s.ID
                               where s.IsActive == true && p.CompanyID == new Guid(mentorResponseViewModel.CompanyId)
                               select s).Count();
                if (empdata < subsDetail.NumberOfExternalMentors)
                {
                    Data.Mentor objMentor = new Data.Mentor();
                    var defaultpassword = ConfigurationManager.AppSettings["DefaultPass"].ToString();
                    string hashpass = PasswordHash.CreateHash(defaultpassword);

                    string userTypeDescription;
                    if (mentorResponseViewModel.CompanyId == "00000000-0000-0000-0000-000000000000")
                        userTypeDescription = EnumHelper.GetDescription(Utility.UserCategory.GlobalMentor);
                    else
                        userTypeDescription = EnumHelper.GetDescription(Utility.UserCategory.CompanyExternalMentor);
                    Guid userTypeID = _context.UserCategories.Where(a => a.Description == userTypeDescription).Select(d => d.Id).SingleOrDefault();

                    UserDetail userDetail = (from ud in _context.UserDetails
                                             join m in _context.Mentors.Where(a => a.IsActive == true) on ud.EmployeeID equals m.ID
                                             where ud.UserName == mentorResponseViewModel.Email
                                             select ud).FirstOrDefault();
                    if (userDetail == null)
                    {
                        Member member = new Member();
                        member.LastName = mentorResponseViewModel.LastName;
                        member.Email = mentorResponseViewModel.Email;
                        member.FirstName = mentorResponseViewModel.FirstName;
                        member.HomePhone = mentorResponseViewModel.Phone;
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
                            objMentor.MemberID = member.MemberID;
                            objMentor.FirstName = mentorResponseViewModel.FirstName;
                            objMentor.LastName = mentorResponseViewModel.LastName;
                            objMentor.Email = mentorResponseViewModel.Email;
                            objMentor.Phone = mentorResponseViewModel.Phone;
                            objMentor.Gender = mentorResponseViewModel.Gender;
                            objMentor.UserType = userTypeID;
                            objMentor.CreatedBy = new Guid(mentorResponseViewModel.CreatedBy);
                            objMentor.CreatedDate = DateTime.UtcNow;
                            objMentor.ModifiedBy = new Guid(mentorResponseViewModel.ModifiedBy);
                            objMentor.ModifiedDate = DateTime.UtcNow;
                            objMentor.IsActive = true;
                            objMentor.ID = Guid.NewGuid();
                            _context.Mentors.Add(objMentor);
                            rowsAffected = _context.SaveChanges();

                            if (rowsAffected > 0)
                            {
                                if (new Guid(mentorResponseViewModel.CompanyId) != Guid.Empty)
                                {
                                    CreateMentorCompanyMapping(new Guid(mentorResponseViewModel.CompanyId), objMentor.ID, objMentor.CreatedBy);
                                }

                                UserDetail user = new UserDetail();
                                user.IsActive = true;
                                user.CreatedDate = DateTime.UtcNow;
                                user.ModifiedDate = System.DateTime.UtcNow;
                                user.CreatedBy = objMentor.CreatedBy;
                                user.ModifiedBy = objMentor.CreatedBy;
                                user.EmployeeID = objMentor.ID;
                                user.Password = hashpass;
                                user.UserName = objMentor.Email;
                                user.IsApproved = true;
                                user.IsRejected = false;
                                user.IsInitialPassword = true;
                                user.UserType = userTypeID;
                                user.ID = Guid.NewGuid();
                                user.RequestDate = DateTime.UtcNow;
                                user.ApprovalDate = DateTime.UtcNow;
                                user.RejectedDate = null;
                                user.IsInitialPassword = true;
                                _context.UserDetails.Add(user);
                                _context.SaveChanges();

                                Employee employee = new Employee()
                                {
                                    Email = objMentor.Email,
                                    FirstName = objMentor.FirstName,
                                    LastName = objMentor.LastName
                                };
                                //SendMail.SendWelcomeEmailToEmployee(employee);
                                WelMailToEmpViewModel welMailToEmp = new WelMailToEmpViewModel()
                                {
                                    Username = employee.FirstName + " " + employee.LastName,
                                    UserEmail = employee.Email
                                };
                                EmailWorkQueue userEmailWorkQueue = new EmailWorkQueue
                                {
                                    WorkItemType = "SendWelcomeEmailToMentor", //tomentor
                                    KeyID = "",
                                    KeyType = "",
                                    SendToEmployee = Guid.Empty,
                                    Subject = "Welcome to Your Sensei",
                                    Body = "",
                                    Template = "SendWelcomeEmailToMentor.html",
                                    TemplateContent = new JavaScriptSerializer().Serialize(welMailToEmp),
                                    Status = "Pending",
                                    CreatedDate = DateTime.UtcNow,
                                    ModifiedDate = DateTime.UtcNow
                                };
                                await _emailWorkQueueService.Save(userEmailWorkQueue);
                            }
                        }

                        return new ResponseViewModel { Code = 200, Message = "Mentor has been created successfully!" };


                    }
                    else
                    {
                        bool isSameCompany = _context.MentorCompanyMappings.Any(a => a.MentorID == userDetail.EmployeeID &&
                            a.CompanyID == new Guid(mentorResponseViewModel.CompanyId));
                        if (!isSameCompany)
                        {
                            if (userTypeID != userDetail.UserType && mentorResponseViewModel.CompanyId == "00000000-0000-0000-0000-000000000000")
                            {
                                objMentor = _context.Mentors.Where(a => a.ID == userDetail.EmployeeID).FirstOrDefault();
                                if (objMentor != null)
                                {
                                    objMentor.UserType = userTypeID;
                                    _context.SaveChanges();

                                    userDetail.UserType = userTypeID;
                                    _context.SaveChanges();
                                }
                            }
                            else
                            {
                                CreateMentorCompanyMapping(new Guid(mentorResponseViewModel.CompanyId), userDetail.EmployeeID, new Guid(mentorResponseViewModel.CreatedBy));
                            }

                            return new ResponseViewModel { Code = 200, Message = "Mentor has been created successfully!" };
                        }
                        return new ResponseViewModel { Code = 403, Message = "Mentor already exist!" };
                    }
                }
                else
                    return new ResponseViewModel { Code = 400, Message = "You Don't add more mentor." };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void CreateMentorCompanyMapping(Guid companyID, Guid mentorID, Guid createdBy)
        {
            MentorCompanyMapping mentorCompanyMapping = new MentorCompanyMapping()
            {
                CompanyID = companyID,
                MentorID = mentorID,
                CreatedBy = createdBy,
                CreatedDate = DateTime.UtcNow
            };
            _context.MentorCompanyMappings.Add(mentorCompanyMapping);
            _context.SaveChanges();
        }

        public async Task<ResponseViewModel> DeleteMentor(string employeeid)
        {
            try
            {
                var data = await _context.Mentors.Where(a => a.ID == new Guid(employeeid)).FirstOrDefaultAsync();
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
                return new ResponseViewModel { Code = 200, Message = "Your Mentor record has been deleted successfully!" };
            }
            catch (Exception)
            {
                return new ResponseViewModel { Code = 400, Message = "Something went wrong please try again after a moment!" };
            }
        }

        public async Task<MentorResponseViewModel> GetMentorById(string empid)
        {
            try
            {
                var data = await _context.Mentors.Where(a => a.ID == new Guid(empid)).Select(a => new MentorResponseViewModel
                {
                    Id = a.ID.ToString(),
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    Phone = a.Phone,
                    IsActive = a.IsActive,
                    Email = a.Email,
                    Gender = a.Gender
                }).FirstOrDefaultAsync();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ResponseViewModel> UpdateMentor(MentorResponseViewModel emp)
        {
            try
            {
                var data = await _context.Mentors.Where(a => a.ID == new Guid(emp.Id)).Select(a => a).FirstOrDefaultAsync();
                if (data != null)
                {
                    data.FirstName = emp.FirstName;
                    data.LastName = emp.LastName;
                    data.IsActive = emp.IsActive;
                    data.Email = emp.Email;
                    data.Phone = emp.Phone;
                    data.Gender = emp.Gender;
                    data.ModifiedBy = new Guid(emp.ModifiedBy);
                    data.ModifiedDate = DateTime.UtcNow;
                    _context.SaveChanges();
                    return new ResponseViewModel { Code = 200, Message = "Mentor has been successfully updated!" };
                }
                else
                {
                    return new ResponseViewModel { Code = 403, Message = "Mentor not Found!" };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<GetMentorByIsActivedatViewModel>> GetMentorsByIsActive(string companyID, bool isActive)
        {
            try
            {
                //var  = await Task.Run(() => _context.usp_GetMentorByIsActive(new Guid(companyID), isActive).ToList());
                

                Guid CompanyID = new Guid(companyID);
                List<GetMentorByIsActivedatViewModel> data = await _context.Database.SqlQuery<GetMentorByIsActivedatViewModel>(
                    "dbo.usp_GetMentorByIsActive @CompanyID = @companyID, @IsActive = @isActive",
                    new SqlParameter("companyID", CompanyID),
                    new SqlParameter("isActive", isActive)).ToListAsync();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> IsExists(string mentorId)
        {
            try
            {
                var data = await _context.Mentors.Where(a => a.ID == new Guid(mentorId)).AnyAsync();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Mentor> GetMentorByEmployeeID(string employeeID)
        {
            try
            {
                var data = await (from m in _context.Mentors
                                  join e in _context.Employees on m.ID equals e.MentorId
                                  where e.ID == new Guid(employeeID) &&
                                      e.IsActive == true &&
                                      m.IsActive == true &&
                                      e.IsExternalMentor == true
                                  select m).FirstOrDefaultAsync();

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
