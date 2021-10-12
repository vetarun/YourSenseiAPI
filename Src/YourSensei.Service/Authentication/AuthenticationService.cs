using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using YourSensei.Data;
using YourSensei.Utility;
using YourSensei.ViewModel;

namespace YourSensei.Service
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly YourSensei_DBEntities _context;
        private readonly ILibraryService _libraryservice;
        private readonly ICompanySettingService _companySetting;
        private readonly IEmployeeService _employeeService;
        private readonly ICreditLogService _creditLogService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IDashboardService _dashboardService;
        private readonly IEmailWorkQueueService _emailWorkQueueService;

        public AuthenticationService(YourSensei_DBEntities context, LibraryService libraryService, CompanySettingService companySetting,
            EmployeeService employeeService, CreditLogService creditLogService, SubscriptionService subscriptionService,DashboardService dashboardService,
            EmailWorkQueueService emailWorkQueueService)
        {
            _context = context;
            _libraryservice = libraryService;
            _companySetting = companySetting;
            _employeeService = employeeService;
            _creditLogService = creditLogService;
            _subscriptionService = subscriptionService;
            _dashboardService = dashboardService;
            _emailWorkQueueService = emailWorkQueueService;
        }

        public async Task<ResponseViewModel> SignUp(SignupInputViewModel comp)
        {
            try
            {
                var date = DateTime.UtcNow;
                string userTypeDescription;
                UserDetail user = new UserDetail();
                List<PendingApprovalViewModel> Approvelist = new List<PendingApprovalViewModel>();
                var defaultpassword = ConfigurationManager.AppSettings["DefaultPass"].ToString();
                string hashpass = PasswordHash.CreateHash(defaultpassword);
                var planidDetail = await _context.SubscriptionPlans.Where(x => x.IsTrialPlan == true).FirstOrDefaultAsync();
                Employee objemp = new Employee();
                var SuperAdmin = await _context.UserDetails.Where(x => x.UserType == new Guid("4BA19173-94CD-4222-AF7C-60C91D446F8E")).FirstOrDefaultAsync();
                if (!String.IsNullOrEmpty(comp.companyname))
                {
                    var result = await _context.UserDetails.Where(d => d.UserName == comp.Email).ToListAsync();
                    if (result.Count == 0)
                    {
                        Data.CompanyDetail objcomp = new Data.CompanyDetail();
                        objcomp.companyname = comp.companyname;
                        objcomp.email = comp.Email;
                        objcomp.websiteaddress = comp.websiteaddress;
                        objcomp.Contact1 = comp.contactnumber;
                        objcomp.Contact2 = comp.contactnumber2 ?? string.Empty;
                        objcomp.AddressLine1 = comp.AddressLine1;
                        objcomp.AddressLine2 = comp.AddressLine2;
                        objcomp.City = comp.City;
                        objcomp.State = comp.State;
                        objcomp.ZipCode = comp.ZipCode;
                        objcomp.Country = comp.Country;
                        objcomp.ID = Guid.NewGuid();
                        _context.CompanyDetails.Add(objcomp);
                        _context.SaveChanges();

                        Member member = new Member();
                        member.LastName = comp.LastName;
                        member.Email = comp.Email;
                        member.FirstName = comp.FirstName;
                        member.HomePhone = comp.HomePhone;
                        member.WorkPhone = comp.WorkPhone;
                        member.AddressLine1 = comp.AddressLine1;
                        member.AddressLine2 = comp.AddressLine2;
                        member.City = comp.City;
                        member.State = comp.State;
                        member.ZipCode = comp.ZipCode;
                        member.Country = comp.Country;
                        member.IsActive = true;
                        member.CreatedBy = new Guid();
                        member.ModifiedBy = new Guid();
                        member.CreatedDate = date;
                        member.ModifiedDate = date;
                        _context.Members.Add(member);
                        _context.SaveChanges();


                        objemp.MemberID = member.MemberID;
                        objemp.FirstName = comp.FirstName;
                        objemp.LastName = comp.LastName;
                        objemp.Email = comp.Email;
                        objemp.UserRoleID = new Guid("F27CB3FA-D876-4FE2-945B-2D8EB63B1155");
                        objemp.OtherRole = "";
                        objemp.IsActive = true;
                        objemp.IsMentor = true;
                        objemp.CreditScore = 0;
                        objemp.CreatedDate = date;
                        objemp.ModifiedDate = date;
                        objemp.CreatedBy = new Guid();
                        objemp.ModifiedBy = new Guid();
                        objemp.Gender = comp.Gender;
                        objemp.HomePhone = comp.HomePhone;
                        objemp.WorkPhone = comp.WorkPhone;
                        objemp.CompanyId = objcomp.ID;
                        objemp.ID = Guid.NewGuid();
                       
                        _context.Employees.Add(objemp);
                        _context.SaveChanges();
                        userTypeDescription = EnumHelper.GetDescription(Utility.UserCategory.CompanyAdmin);
                        Guid userTypeID = _context.UserCategories.Where(a => a.Description == userTypeDescription).Select(d => d.Id).SingleOrDefault();


                        user.EmployeeID = objemp.ID;
                        user.UserName = comp.Email;
                        user.Password = hashpass;
                        user.IsActive = true;
                        user.CreatedDate = date;
                        user.ModifiedDate = date;
                        user.CreatedBy = new Guid();
                        user.ModifiedBy = new Guid();
                        user.UserType = userTypeID;
                        user.ID = Guid.NewGuid();
                        user.RequestDate = date;
                        user.IsApproved = false;
                        user.ApprovalDate = null;
                        user.IsRejected = false;
                        user.RejectedDate = null;
                        user.IsInitialPassword = true;

                        _context.UserDetails.Add(user);
                        _context.SaveChanges();
                        await _subscriptionService.SubscribePlanToUser(Convert.ToString(user.ID), Convert.ToString(objcomp.ID), planidDetail.ID, planidDetail.NumberOfDays);
                        //SendMail.SendUserForSignup(comp);
                        EmailWorkQueue emailWorkQueue = new EmailWorkQueue
                        {
                            WorkItemType = "SendUserForSignup",
                            KeyID = "",
                            KeyType = "",
                            SendToEmployee = user.ID,
                            Subject = "Sign Up Successful. Welcome to YourSensei!",
                            Body = "",
                            Template = "SendUserForSignup.html",
                            TemplateContent = "",
                            Status = "Pending",
                            CreatedDate = DateTime.UtcNow,
                            ModifiedDate = DateTime.UtcNow
                        };
                        await _emailWorkQueueService.Save(emailWorkQueue);
                        //SendMail.SendAdminForApproval(comp, SuperAdmin);
                        EmailWorkQueue emailWorkQueue1 = new EmailWorkQueue
                        {
                            WorkItemType = "SendAdminForApproval",
                            KeyID = "",
                            KeyType = "",
                            SendToEmployee = user.ID,
                            Subject = "Approval Request",
                            Body = "",
                            Template = "SendAdminForApproval.html",
                            TemplateContent = "",
                            Status = "Pending",
                            CreatedDate = DateTime.UtcNow,
                            ModifiedDate = DateTime.UtcNow
                        };
                        await _emailWorkQueueService.Save(emailWorkQueue1);

                    }
                    else
                    {
                        return new ResponseViewModel { Code = 400, Message = "Email already exist!" };
                    }
                }
                else
                {

                    var result = _context.UserDetails.Where(d => d.UserName == comp.Email).ToList();
                    if (result.Count == 0)
                    {
                        Member member = new Member();
                        member.LastName = comp.LastName;
                        member.Email = comp.Email;
                        member.FirstName = comp.FirstName;
                        member.HomePhone = comp.HomePhone;
                        member.WorkPhone = comp.WorkPhone;
                        member.AddressLine1 = comp.AddressLine1;
                        member.AddressLine2 = comp.AddressLine2;
                        member.City = comp.City;
                        member.State = comp.State;
                        member.ZipCode = comp.ZipCode;
                        member.Country = comp.Country;
                        member.IsActive = true;
                        member.CreatedBy = new Guid();
                        member.ModifiedBy = new Guid();
                        member.CreatedDate = date;
                        member.ModifiedDate = date;
                        _context.Members.Add(member);
                        _context.SaveChanges();
                        
                        objemp.MemberID = member.MemberID;
                        objemp.FirstName = comp.FirstName;
                        objemp.LastName = comp.LastName;
                        objemp.Email = comp.Email;
                        objemp.UserRoleID = new Guid("00000000-0000-0000-0000-000000000000");
                        objemp.IsActive = true;
                        objemp.IsMentor = false;
                        objemp.CreditScore = 0;
                        objemp.CreatedDate = date;
                        objemp.IsExternalMentor = true;
                        objemp.MentorId = new Guid(comp.mentorId);
                        objemp.ModifiedDate = date;
                        objemp.CreatedBy = new Guid();
                        objemp.ModifiedBy = new Guid();
                        objemp.Gender = comp.Gender;
                        objemp.HomePhone = comp.HomePhone;
                        objemp.WorkPhone = comp.WorkPhone;
                        objemp.CompanyId = new Guid("00000000-0000-0000-0000-000000000000");
                        objemp.ID = Guid.NewGuid();
                        _context.Employees.Add(objemp);
                        _context.SaveChanges();

                        var newemp = await _context.Employees.Where(d => d.Email == comp.Email).FirstOrDefaultAsync();
                        if (newemp != null)
                        {
                            userTypeDescription = EnumHelper.GetDescription(Utility.UserCategory.Individual);
                            Guid userTypeID = _context.UserCategories.Where(a => a.Description == userTypeDescription).Select(d => d.Id).SingleOrDefault();

                            user.IsActive = true;
                            user.CreatedDate = DateTime.UtcNow;
                            user.ModifiedDate = System.DateTime.UtcNow;
                            user.CreatedBy = objemp.ID;
                            user.ModifiedBy = objemp.ID;
                            user.EmployeeID = newemp.ID;
                            user.Password = hashpass;
                            user.UserName = objemp.Email;
                            user.ID = Guid.NewGuid();
                            user.UserType = userTypeID;
                            user.RequestDate = date;
                            user.IsApproved = false;
                            user.ApprovalDate = null;
                            user.IsRejected = false;
                            user.RejectedDate = null;
                            user.IsInitialPassword = true;
                            _context.UserDetails.Add(user);
                            _context.SaveChanges();
                        }
                        await _subscriptionService.SubscribePlanToUser(Convert.ToString(user.ID), string.Empty, planidDetail.ID, planidDetail.NumberOfDays);
                        //SendMail.SendUserForSignup(comp);
                        EmailWorkQueue emailWorkQueue = new EmailWorkQueue
                        {
                            WorkItemType = "SendUserForSignup",
                            KeyID = "",
                            KeyType = "",
                            SendToEmployee = user.ID,
                            Subject = "Sign Up Successful. Welcome to YourSensei!",
                            Body = "",
                            Template = "SendUserForSignup.html",
                            TemplateContent = "",
                            Status = "Pending",
                            CreatedDate = DateTime.UtcNow,
                            ModifiedDate = DateTime.UtcNow
                        };
                        await _emailWorkQueueService.Save(emailWorkQueue);
                        //SendMail.SendAdminForApproval(comp, SuperAdmin);
                        EmailWorkQueue emailWorkQueue1 = new EmailWorkQueue
                        {
                            WorkItemType = "SendAdminForApproval",
                            KeyID = "",
                            KeyType = "",
                            SendToEmployee = user.ID,
                            Subject = "Approval Request",
                            Body = "",
                            Template = "SendAdminForApproval.html",
                            TemplateContent = "",
                            Status = "Pending",
                            CreatedDate = DateTime.UtcNow,
                            ModifiedDate = DateTime.UtcNow
                        };
                        await _emailWorkQueueService.Save(emailWorkQueue1);
                    }
                    else
                    {
                        return new ResponseViewModel { Code = 403, Message = "Email already exist!" };
                    }
                }
                
                
                PendingApprovalViewModel approve = new PendingApprovalViewModel
                {
                    UserDetailID = user.ID,
                    IsApproved = true,
                    CallType = "Approve",
                    CompanyID=objemp.CompanyId
                };
                Approvelist.Add(approve);
                await AcceptRejectList(Approvelist);
                return !String.IsNullOrEmpty(comp.companyname) ? new ResponseViewModel { Code = 200, Message = "Your company has been created successfully!" } : new ResponseViewModel { Code = 200, Message = "Employee has been created successfully!" };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<LoginResponse> Login(string email, string pass)
        {
            try
            {
                bool valid = false;
                DateTime? subsExpireDate = new DateTime();
                Guid mentorCompanyID = new Guid();
                var IsUserExist = await _context.UserDetails.FirstOrDefaultAsync(d => d.UserName == email);

                if (IsUserExist != null)
                {
                    Employee EmployeeDetails = IsUserExist.EmployeeID != new Guid("00000000-0000-0000-0000-000000000000") ?
                        _context.Employees.FirstOrDefault(d => d.IsActive == true && d.ID == IsUserExist.EmployeeID) : null;

                    Mentor mentor = IsUserExist.EmployeeID != new Guid("00000000-0000-0000-0000-000000000000") ?
                        _context.Mentors.FirstOrDefault(d => d.IsActive == true && d.ID == IsUserExist.EmployeeID) : null;

                    if (!IsUserExist.IsApproved)
                    {
                        return new LoginResponse { Code = 400, Message = "Your Account is pending approval. You will receive an email once it is approved. We appreciate your patience." };
                    }

                    valid = PasswordHash.ValidatePassword(pass, IsUserExist.Password);
                    if (!valid)
                    {
                        return new LoginResponse { Code = 400, Message = "password does not matched" };
                    }
                    // if account is deactivated then redirect to login page
                    if (!IsUserExist.IsActive)
                    {
                        return new LoginResponse { Code = 400, Message = "Your Account is Deactivated By the Administrator" };
                    }

                    bool IsDollarApprover = false;
                    SubscriptionPlan subscriptionPlan = new SubscriptionPlan();
                    if (EmployeeDetails != null)
                    {
                        Subscription subscription = null;
                        if (EmployeeDetails.CompanyId == new Guid("00000000-0000-0000-0000-000000000000"))
                        {
                            subscription = _context.Subscriptions.Where(x => x.UserDetailID == IsUserExist.ID && x.IsExpired == false && x.IsActivated == true).FirstOrDefault();
                            if (subscription != null)
                                subsExpireDate = subscription.ExpiryDate;
                            else
                                subsExpireDate = DateTime.MinValue;

                            subscriptionPlan = await _subscriptionService.GetSubscribedPlan(Guid.Empty, IsUserExist.ID);
                        }
                        else
                        {
                            subscription = _context.Subscriptions.Where(x => x.CompanyID == EmployeeDetails.CompanyId && x.IsExpired == false && x.IsActivated == true).FirstOrDefault();
                            if (subscription != null)
                                subsExpireDate = subscription.ExpiryDate;
                            else
                                subsExpireDate = DateTime.MinValue;
                            subscriptionPlan = await _subscriptionService.GetSubscribedPlan(EmployeeDetails.CompanyId, Guid.Empty);
                        }

                        IsDollarApprover = (from cs in _context.CompanySettings
                                            where cs.CompanyId == EmployeeDetails.CompanyId
                                              && cs.A3DollarApprover == EmployeeDetails.ID
                                              && cs.IsActive == true
                                            select cs).Any();
                    }
                    else if (mentor != null)
                    {
                        subsExpireDate = DateTime.UtcNow;

                        IsDollarApprover = (from cs in _context.CompanySettings
                                            join mcm in _context.MentorCompanyMappings on cs.CompanyId equals mcm.CompanyID
                                            where mcm.MentorID == mentor.ID
                                                && cs.A3DollarApprover == mentor.ID
                                                && cs.IsActive == true
                                            select cs).Any();
                    }

                    string[] FeaturesAllowedArray = subscriptionPlan.FeaturesAllowed != null ? Convert.ToString(subscriptionPlan.FeaturesAllowed).
                            Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries) : new string[] { "" };

                    LoginResponse loginResponse = new LoginResponse()
                    {
                        Email = EmployeeDetails != null ? EmployeeDetails.Email : IsUserExist.UserName,
                        Name = EmployeeDetails != null ? EmployeeDetails.FirstName + " " + EmployeeDetails.LastName : mentor != null ? mentor.FirstName + " " + mentor.LastName : _context.UserCategories.Find(IsUserExist.UserType).Description,
                        UserId = IsUserExist.ID,
                        EmployeeID = EmployeeDetails != null ? EmployeeDetails.ID.ToString() : "00000000-0000-0000-0000-000000000000",
                        UserRole = EmployeeDetails != null ? EmployeeDetails.UserRoleID : new Guid("00000000-0000-0000-0000-000000000000"),
                        CompanyId = EmployeeDetails != null ? EmployeeDetails.CompanyId : new Guid("00000000-0000-0000-0000-000000000000"),
                        Usertypeid = IsUserExist.UserType,
                        UserTypeName = _context.UserCategories.Find(IsUserExist.UserType).Description,
                        MentorID = EmployeeDetails != null && EmployeeDetails.IsMentor ? EmployeeDetails.MentorId.ToString() : mentor != null ? mentor.ID.ToString() : "00000000-0000-0000-0000-000000000000",
                        IsInternalMentor = EmployeeDetails != null ? EmployeeDetails.IsMentor : false,
                        IsInitialPassword = IsUserExist.IsInitialPassword,
                        phone = EmployeeDetails != null ? EmployeeDetails.HomePhone : "",
                        IsDollarApprover = IsDollarApprover,
                        SubscriptionExpiryDate = subsExpireDate,
                        ID = subscriptionPlan.ID,
                        PlanName = Convert.ToString(subscriptionPlan.Name),
                        Description = Convert.ToString(subscriptionPlan.Description),
                        NumberOfDays = subscriptionPlan.NumberOfDays,
                        NumberOfEmployees = subscriptionPlan.NumberOfEmployees,
                        NumberOfExternalMentors = subscriptionPlan.NumberOfExternalMentors,
                        Price = subscriptionPlan.Price,
                        FeaturesAllowed = Convert.ToString(subscriptionPlan.FeaturesAllowed),
                        FeaturesAllowedArray = FeaturesAllowedArray,
                        FeaturesAllowedList = new List<string>(),
                        IsTrialPlan = subscriptionPlan.IsTrialPlan,
                        IsActive = subscriptionPlan.IsActive,
                        CreatedBy = Convert.ToString(subscriptionPlan.CreatedBy),
                        CreatedDate = subscriptionPlan.CreateDate,
                        ModifiedBy = Convert.ToString(subscriptionPlan.ModifiedBy),
                        ModifiedDate = subscriptionPlan.ModifiedDate,
                        Code = 200,
                        Message = "Successfully Login"
                    };

                    if (loginResponse.FeaturesAllowed != null)
                    {
                        string[] names = Enum.GetNames(typeof(FeaturesAllowed));
                        foreach (string name in names)
                        {
                            if (loginResponse.FeaturesAllowed.Contains("," + name + ","))
                            {
                                string enumDescription = EnumHelper.GetDescription((FeaturesAllowed)Enum.Parse(typeof(FeaturesAllowed), name));
                                loginResponse.FeaturesAllowedList.Add(enumDescription);
                                if (string.IsNullOrWhiteSpace(loginResponse.FinalFeaturesAllowed))
                                    loginResponse.FinalFeaturesAllowed = enumDescription;
                                else
                                    loginResponse.FinalFeaturesAllowed = loginResponse.FinalFeaturesAllowed + ", " + enumDescription;
                            }
                        }
                    }

                    return loginResponse;
                }
                else
                {
                    return new LoginResponse
                    {
                        Code = 400,
                        Message = "Email not Found"
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ResponseViewModel> ForgotPassword(string email)
        {
            try
            {
                ResponseViewModel rtnobj = new ResponseViewModel();

                var result = await _context.UserDetails.Where(d => d.UserName == email).ToListAsync();
                if (result.Count > 0)
                {
                    EmailWorkQueue emailWorkQueue = new EmailWorkQueue()
                    {
                        WorkItemType = "PasswordReset",
                        KeyID = "",
                        KeyType = "",
                        SendToEmployee = result[0].ID,
                        Subject = "Resest Password",
                        Body = "",
                        Template = "PasswordReset.html",
                        TemplateContent = "",
                        Status = "Pending",
                        CreatedDate = DateTime.UtcNow,
                        ModifiedDate = DateTime.UtcNow
                    };
                    await _emailWorkQueueService.Save(emailWorkQueue);

                    //SendMail.SendPasswordResetEmail(email);
                    rtnobj.Code = 200;
                    rtnobj.Message = "Reset link has been sent to your email!";
                }
                else
                {
                    rtnobj.Code = 400;
                    rtnobj.Message = "Email does not exist!";
                }

                return rtnobj;
            }
            catch (Exception ex)
            {
                return new ResponseViewModel { Code = 400, Message = "Some thing went worng please try after a moment" };
            }
        }

        public async Task<ResponseViewModel> ResetPassword(string link, string password, string email, string oldpassword)
        {
            try
            {
                ResponseViewModel rtnobj = new ResponseViewModel();
                if (link != "null")
                {

                    var fixlink = link.Split(' ');
                    string newlink = "";
                    foreach (var item in fixlink)
                    {
                        newlink = newlink + "+" + item;
                    }
                    string encryptdata1 = Encryption.Decrypt(newlink.Substring(1, newlink.Length - 1), "123456789");

                    var userdata = encryptdata1.Split('&');
                    var ExDate = userdata[1].Split(':')[1] + ":" + userdata[1].Split(':')[2] + ":" + userdata[1].Split(':')[3];
                    var useremail = userdata[0].Split(':')[1];
                    DateTime expiryDate = Convert.ToDateTime(ExDate);

                    if (expiryDate <= System.DateTime.UtcNow)
                    {
                        rtnobj.Code = 400;
                        rtnobj.Message = "Your reset link has been expired!";

                    }
                    else
                    {
                        rtnobj.Code = 200;
                        rtnobj.Message = "You can reset your password here!";

                    }
                    if (password != "null")
                    {
                        var newpas = PasswordHash.CreateHash(password);
                        var result = await _context.UserDetails.Where(d => d.UserName == useremail).ToListAsync();
                        if (result.Count > 0)
                        {
                            var result1 = _context.UserDetails.FirstOrDefault(d => d.UserName == useremail);
                            result1.Password = newpas;
                            result1.IsInitialPassword = false;
                            result1.ModifiedBy = result1.ID;
                            result1.ModifiedDate = DateTime.UtcNow;
                            _context.SaveChanges();

                            Employee employee = _context.Employees.FirstOrDefault(d => d.Email == useremail);
                            //SendMail.SendChangePasswordEmail(employee);

                            ChangePasswordViewModel changePassword = new ChangePasswordViewModel()
                            {
                                Username = employee.FirstName + " " + employee.LastName,
                                UserEmail = employee.Email
                            };
                            EmailWorkQueue userEmailWorkQueue = new EmailWorkQueue
                            {
                                WorkItemType = "SendChangePasswordEmail",
                                KeyID = "",
                                KeyType = "",
                                SendToEmployee = Guid.Empty,
                                Subject = "Did you change your password?",
                                Body = "",
                                Template = "SendChangePasswordEmail.html",
                                TemplateContent = new JavaScriptSerializer().Serialize(changePassword),
                                Status = "Pending",
                                CreatedDate = DateTime.UtcNow,
                                ModifiedDate = DateTime.UtcNow
                            };
                            await _emailWorkQueueService.Save(userEmailWorkQueue);

                            rtnobj.Code = 200;
                            rtnobj.Message = "Your Password has been reset successfully!";
                        }
                        else
                        {
                            rtnobj.Code = 400;
                            rtnobj.Message = "Email Address does not exist!";
                        }
                    }
                }
                else
                {
                    var IsUserExist = await _context.UserDetails.Where(x => x.UserName == email).FirstOrDefaultAsync();
                    var valid = PasswordHash.ValidatePassword(oldpassword, IsUserExist.Password);
                    if (!valid)
                    {
                        rtnobj.Code = 400;
                        rtnobj.Message = "Old Password is Incorrect";
                        return rtnobj;
                    }
                    if (password != "null")
                    {
                        var newpas = PasswordHash.CreateHash(password);
                        if (IsUserExist != null)
                        {

                            IsUserExist.Password = newpas;
                            IsUserExist.IsInitialPassword = false;
                            IsUserExist.ModifiedBy = IsUserExist.ID;
                            IsUserExist.ModifiedDate = DateTime.UtcNow;
                            _context.SaveChanges();

                            Employee employee = _context.Employees.FirstOrDefault(d => d.Email == email);
                            //SendMail.SendChangePasswordEmail(employee);
                            ChangePasswordViewModel changePassword = new ChangePasswordViewModel()
                            {
                                Username = employee.FirstName + " " + employee.LastName,
                                UserEmail = employee.Email
                            };
                            EmailWorkQueue userEmailWorkQueue = new EmailWorkQueue
                            {
                                WorkItemType = "SendChangePasswordEmail",
                                KeyID = "",
                                KeyType = "",
                                SendToEmployee = Guid.Empty,
                                Subject = "Did you change your password?",
                                Body = "",
                                Template = "SendChangePasswordEmail.html",
                                TemplateContent = new JavaScriptSerializer().Serialize(changePassword),
                                Status = "Pending",
                                CreatedDate = DateTime.UtcNow,
                                ModifiedDate = DateTime.UtcNow
                            };
                            await _emailWorkQueueService.Save(userEmailWorkQueue);

                            rtnobj.Code = 200;
                            rtnobj.Message = "Your Password has been reset successfully!";
                        }
                        else
                        {
                            rtnobj.Code = 400;
                            rtnobj.Message = "Email Address does not exist!";
                        }
                    }
                }


                return rtnobj;
            }
            catch (Exception ex)
            {
                return new ResponseViewModel { Code = 400, Message = "Some thing went worng please try after a moment" };
            }
        }

        public async Task<List<usp_GetRegistrationDetails_Result>> AllPendingApproval(bool approved, bool rejected)
        {
            var allapprovals = await Task.Run(() => _context.usp_GetRegistrationDetails(approved, rejected).ToList());
            return allapprovals;
        }

        public async Task<ResponseViewModel> AcceptRejectList(List<PendingApprovalViewModel> lst)
        {
            try
            {

                List<CompanyLibraryBook> superadminbooks = await _context.CompanyLibraryBooks.Where(a => a.CompanyID == new Guid("00000000-0000-0000-0000-000000000000") && a.ParentBookID == null).Select(a => a).ToListAsync();
                foreach (var item in lst)
                {
                    if (item.CallType == "Approve")
                    {
                        Employee employee = new Employee();
                        var data = await _context.UserDetails.Where(t => t.ID == item.UserDetailID).Select(t => t).FirstOrDefaultAsync();
                        if (data != null)
                        {
                            data.IsApproved = item.IsApproved;
                            data.ApprovalDate = DateTime.UtcNow;
                            data.ModifiedBy = item.UserDetailID;
                            data.ModifiedDate = DateTime.UtcNow;
                            await _context.SaveChangesAsync();

                            EmployeeResponseViewModel employeeResponseViewModel = await _employeeService.GetEmployeeById(data.EmployeeID.ToString());
                            employeeResponseViewModel.userDetialID = data.ID.ToString();
                            await _creditLogService.SaveCreditLog(employeeResponseViewModel, 0, data.ID.ToString(), "EmployeeInitialCredit");

                            employee.Email = employeeResponseViewModel.Email;
                            employee.FirstName = employeeResponseViewModel.FirstName;
                            employee.LastName = employeeResponseViewModel.LastName;
                        }

                        foreach (var sabook in superadminbooks)
                        {
                            //sabook.CompanyID = item.CompanyID;
                            //sabook.UserDetailID = null;
                            _libraryservice.ReplicateGlobalBookForCompanyIndividual(sabook, item.CompanyID, item.UserDetailID);
                        }
                        CompanySettingViewModel settingobj = new CompanySettingViewModel
                        {
                            GlobalAverageBookRating = true,
                            IsActive = true,
                            CompanyId = item.CompanyID.ToString(),
                            CreatedBy = item.UserDetailID.ToString(),
                            CreatedDate = DateTime.UtcNow,
                            GlobalBookList = true,
                            GlobalMentor = true,
                            IsMentorMandatory = true,
                            ModifiedBy = item.UserDetailID.ToString(),
                            ModifiedDate = DateTime.UtcNow,
                            A3DollarApprover = item.UserDetailID.ToString()
                        };

                       
                        _dashboardService.ReplicateBeltForCompanyIndividual(item.CompanyID, item.UserDetailID);
                        await _companySetting.AddCompanySetting(settingobj);


                        //SendMail.SendWelcomeEmailToEmployee(employee);
                        WelMailToEmpViewModel welMailToEmp = new WelMailToEmpViewModel()
                        {
                            Username = employee.FirstName + " " + employee.LastName,
                            UserEmail = employee.Email
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
                    else
                    {
                        var data = await _context.UserDetails.Where(t => t.ID == item.UserDetailID).Select(t => t).FirstOrDefaultAsync();
                        if (data != null)
                        {
                            data.IsRejected = true;
                            data.RejectedDate = DateTime.UtcNow;
                            data.ModifiedBy = item.UserDetailID;
                            data.ModifiedDate = DateTime.UtcNow;
                            await _context.SaveChangesAsync();

                            EmailWorkQueue emailWorkQueue = new EmailWorkQueue()
                            {
                                WorkItemType = "RejectedEmailFromSensei",
                                KeyID = "",
                                KeyType = "",
                                SendToEmployee = data.ID,
                                Subject = "Intimation for rejection",
                                Body = "",
                                Template = "RejectedEmailFromSensei.html",
                                TemplateContent = "",
                                Status = "Pending",
                                CreatedDate = DateTime.UtcNow,
                                ModifiedDate = DateTime.UtcNow
                            };
                            await _emailWorkQueueService.Save(emailWorkQueue);
                        }
                       // SendMail.SendRejectedEmailFromSensei(data.UserName);


                    }
                }

                return new ResponseViewModel { Code = 200, Message = "Success" };
            }
            catch (Exception ex)
            {

                return new ResponseViewModel { Code = 400, Message = "Some thing went worng please try after a moment" };
            }
        }

        public async Task<ResponseViewModel> SendSupport(TechSupportInputViewModel input)
        {
            TechnicalSupport supp = new TechnicalSupport()
            {
                FirstName = input.firstName,
                LastName = input.lastName,
                Phone = input.phone,
                HeloBox = input.helpbox,
                Email = input.email,
                AddionalDetail = input.additional,
                CreatedDate = DateTime.UtcNow
            };
            _context.TechnicalSupports.Add(supp);
            await _context.SaveChangesAsync();

            TechnicalSupportEmailViewModel technicalSupportEmailViewModel = new TechnicalSupportEmailViewModel()
            {
                InputEmail = input.email,
                InputName = input.firstName + " " + input.lastName,
                InputPhone = input.phone,
                InputHelpBox = input.helpbox,
                InputAdditional = input.additional,
            };
            EmailWorkQueue emailWorkQueue = new EmailWorkQueue()
            {
                WorkItemType = "TechnicalSupportEmail",
                KeyID = "",
                KeyType = "",
                SendToEmployee = Guid.Empty,
                Subject = "Technical Support",
                Body = "",
                Template = "TechSupportEmail.html",
                TemplateContent = new JavaScriptSerializer().Serialize(technicalSupportEmailViewModel),
                Status = "Pending",
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };
            await _emailWorkQueueService.Save(emailWorkQueue);
            //SendMail.SendTechnivalSupportEmail(input);
            return new ResponseViewModel { Code = 200, Message = "Successfully Submitted" };
        }

        public async Task<List<PaymentCardDetail>> GetPaymentCardList(int id,string companyId,string UserId)
        {
            try
            {
                List<PaymentCardDetail> cardList;
                if (!string.IsNullOrWhiteSpace(companyId))
                {
                    cardList = await (from sp in _context.PaymentCardDetails
                                                              where sp.IsActive == true && ((sp.ID == id && sp.CompanyID==new Guid(companyId)) || (id == 0 && sp.CompanyID == new Guid(companyId)))
                                                              select sp).ToListAsync();
                }
                else
                {
                    cardList = await (from sp in _context.PaymentCardDetails
                                      where sp.IsActive == true && ((sp.ID == id && sp.UserDetailID == new Guid(UserId)) || (id == 0 && sp.UserDetailID == new Guid(UserId)))
                                      select sp).ToListAsync();
                }
               
                return cardList;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<ResponseViewModel> AddUpdateCardDetails(CardDetailsInputViewModel input)
        {
            try
            {
                if (input.ID == 0)
                {
                    PaymentCardDetail card = new PaymentCardDetail();

                    card.CardNumber = input.CardNumber;
                    card.CardType = input.CardType;
                    card.NameOnCard = input.NameOnCard;
                    card.ValidThru = input.ValidThru;
                    if (!String.IsNullOrWhiteSpace(input.CompanyID))
                        card.CompanyID = new Guid(input.CompanyID);
                    else
                        card.UserDetailID = new Guid(input.UserDetailID);
                    card.IsActive = true;
                    card.CreatedBy = new Guid(input.UserDetailID);
                    card.CreatedDate = DateTime.Now;
                    _context.PaymentCardDetails.Add(card);
                    _context.SaveChanges();
                    return new ResponseViewModel { Code = 200, Message = "Card has been succefully added" };
                }
                else
                {
                    var isCardExist = await _context.PaymentCardDetails.FindAsync(input.ID);
                    if (isCardExist !=null)
                    {
                        isCardExist.CardNumber = input.CardNumber;
                        isCardExist.CardType = input.CardType;
                        isCardExist.NameOnCard = input.NameOnCard;
                        isCardExist.ValidThru = input.ValidThru;
                        isCardExist.IsActive = input.IsActive;
                        if (!String.IsNullOrWhiteSpace(input.CompanyID))
                            isCardExist.CompanyID = new Guid(input.CompanyID);
                        else
                            isCardExist.UserDetailID = new Guid(input.UserDetailID);
                        //isCardExist.IsActive = true;
                        isCardExist.ModifiedBy = new Guid(input.UserDetailID);
                        isCardExist.ModifiedDate = DateTime.Now;
                       
                        _context.SaveChanges();
                        return new ResponseViewModel { Code = 200, Message = "Card has been succefully updated" };
                    }
                    else
                    {
                        return new ResponseViewModel { Code = 403, Message = "Card has not found" };
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<UserDetail> GetSuperAdminDetail()
        {
            return await _context.UserDetails.Where(x => x.UserType == new Guid("4BA19173-94CD-4222-AF7C-60C91D446F8E")).FirstOrDefaultAsync();
        }

        public async Task<SignupInputViewModel> GetCompanyDetailByUserId(string userid)
        {
            SignupInputViewModel signupInputViewModel = new SignupInputViewModel();
            var result = await _context.UserDetails.Where(x => x.ID == new Guid(userid)).FirstOrDefaultAsync();
            if (result.UserType.ToString().ToUpper() == "99F9AEB1-9BE6-4E82-8671-CA3DF4DF16CB")
            {
               var cresult = (from cd in _context.CompanyDetails join em in _context.Employees on cd.ID equals em.CompanyId
                       where em.ID == result.EmployeeID                         
                       select new { cd.companyname,em.FirstName }).FirstOrDefault();
                if (cresult != null)
                {
                    signupInputViewModel.FirstName = cresult.FirstName;
                    signupInputViewModel.companyname = cresult.companyname;
                }
                
            }
            else
            {
                var cresult = (from em in _context.Employees                               
                               where em.ID == result.EmployeeID
                               select new { em.FirstName }).FirstOrDefault();
                if (cresult != null)
                {
                    signupInputViewModel.FirstName = cresult.FirstName;
                }

            }
            return signupInputViewModel;
        }
        public async Task<CompanyDetail> GetCompanyDetailByCompanyId(string CompanyId)
        {
            return await _context.CompanyDetails.Where(x => x.ID == new Guid(CompanyId)).FirstOrDefaultAsync(); ;
        }

        public async Task<List<GetUserDetailResponseViewModel>> GetUserDetails(  string companyId)
        {
            try
            {
                List<GetUserDetailResponseViewModel> list = new List<GetUserDetailResponseViewModel>();
                List<EmployeeResponseViewModel> emplist = new List<EmployeeResponseViewModel>();
                if(companyId != "null" &&  companyId != string.Empty)
                {
                     emplist = await _employeeService.GetAllEmployee(companyId);
                }
                else
                {
                    emplist = await _employeeService.GetAllEmployee(string.Empty);
                }
                foreach (var item in emplist)
                {
                    GetUserDetailResponseViewModel mod = new GetUserDetailResponseViewModel();
                    mod.email = item.Email;
                    mod.firstname = item.FirstName;
                    mod.lastname = item.LastName;
                    mod.totalcredit = item.Credit;
                    var companydetail = _context.CompanyDetails.FirstOrDefault(x => x.ID == new Guid(item.CompanyId));
                    mod.company =  companydetail != null ? companydetail.companyname : "";
                    mod.companyId = companydetail != null ? companydetail.ID.ToString() : "";
                    mod.totala3 = (from a in _context.TrainingEventAttendees
                                   join c in _context.TrainingEvents on a.TrainigEventID equals c.ID
                                   where a.EmployeeID ==  new Guid(item.Id) && c.Isclosed == true &&
                                    c.TrainingEventFormatID == new Guid("6F9F04CC-198E-479C-A93F-6C3C0A359194")
                                   select c).Count();

                    mod.totalkaizen = (from a in _context.TrainingEventAttendees
                                       join c in _context.TrainingEvents on a.TrainigEventID equals c.ID
                                       where a.EmployeeID == new Guid(item.Id) && c.Isclosed == true &&
                                        c.TrainingEventFormatID == new Guid("5518993A-EFC0-4AD0-BCD7-BEAEA42CC2CE")
                                       select c).Count();
                    var creditlogs = _context.CreditLogs.Where(x => x.UserDetailID == new Guid(item.userDetialID)).FirstOrDefault();
                    mod.lastcreditearn = creditlogs != null ? creditlogs.Credit : Convert.ToDecimal("0");
                    mod.beltname = _dashboardService.getBeltName(mod.totalcredit, mod.totala3, mod.totalkaizen, item.CompanyId, item.userDetialID).BeltName;
                    list.Add(mod);
                }
                return list;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
