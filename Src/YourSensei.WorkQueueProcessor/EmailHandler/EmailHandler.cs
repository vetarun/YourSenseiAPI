using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using YourSensei.Data;
using YourSensei.Service;
using YourSensei.Utility;
using YourSensei.ViewModel;

namespace YourSensei.WorkQueueProcessor
{
    public class EmailHandler : IEmailHandler
    {
        private readonly IEmployeeService _employeeService;
        private readonly ITrainingEventService _trainingservices;
        private readonly ISubscriptionService _subscriptionservices;
        private readonly IAuthenticationService _authenticationService;
        private readonly YourSensei_DBEntities _context;

        public EmailHandler(EmployeeService employeeService, TrainingEventService trainingservices, SubscriptionService subscriptionService, AuthenticationService authenticationService, YourSensei_DBEntities context)
        {
            _employeeService = employeeService;
            _trainingservices = trainingservices;
            _subscriptionservices = subscriptionService;
            _authenticationService = authenticationService;
            _context = context;
        }

        public string ProcessPasswordResetRequest(EmailWorkQueue emailWorkQueue)
        {
            Task<Employee> taskEmployee = Task.Run(() => _employeeService.GetEmployeeByUserDetailID(emailWorkQueue.SendToEmployee));
            taskEmployee.Wait();
            Employee employee = taskEmployee.Result;

            if (employee != null)
            {
                string templateContent = ReadTemplateContent(emailWorkQueue.Template);
                templateContent = templateContent.Replace("[Username]", employee.FirstName + " " + employee.LastName);
                templateContent = templateContent.Replace("[AngularURL]", ConfigurationManager.AppSettings["AngularUrl"].ToString());

                DateTime expredDate = System.DateTime.Now.AddHours(48.0);
                string dataToEncrypt = "email:" + employee.Email + "&expiredate:" + expredDate;
                string encryptData = Encryption.Encrypt(dataToEncrypt, ConfigurationManager.AppSettings["PassPhrase"].ToString());
                templateContent = templateContent.Replace("[EncryptData]", encryptData);

                EmailHelperInputModel emailHelperInputModel = new EmailHelperInputModel()
                {
                    Subject = emailWorkQueue.Subject,
                    Email = employee.Email,
                    Message = templateContent
                };
                SendEmail(emailHelperInputModel);
                return EmailWorkItemStatus.Completed.ToString();
            }
            return EmailWorkItemStatus.Failed.ToString();
        }

        public string ProcessBookReadToMentor(EmailWorkQueue emailWorkQueue)
        {
            BookReadToMentorViewModel bookReadToMentorViewModel = new JavaScriptSerializer().Deserialize<BookReadToMentorViewModel>(emailWorkQueue.TemplateContent);
            if (bookReadToMentorViewModel != null)
            {
                string templateContent = ReadTemplateContent(emailWorkQueue.Template);
                templateContent = templateContent.Replace("[Username]", bookReadToMentorViewModel.ToEmployeeName);
                templateContent = templateContent.Replace("[StudentName]", bookReadToMentorViewModel.StudentName);
                templateContent = templateContent.Replace("[BookTitle]", bookReadToMentorViewModel.BookTitle);
                templateContent = templateContent.Replace("[AngularUrl]", bookReadToMentorViewModel.AngularURL);

                EmailHelperInputModel emailHelperInputModel = new EmailHelperInputModel()
                {
                    Subject = bookReadToMentorViewModel.Subject,
                    Email = bookReadToMentorViewModel.ToEmployeeEmail,
                    Message = templateContent
                };
                SendEmail(emailHelperInputModel);
                return EmailWorkItemStatus.Completed.ToString();
            }

            return EmailWorkItemStatus.Failed.ToString();
        }

        public string ProcessBookReadToEmployee(EmailWorkQueue emailWorkQueue)
        {
            BookReadToEmployeeViewModel bookReadToMentorViewModel = new JavaScriptSerializer().Deserialize<BookReadToEmployeeViewModel>(emailWorkQueue.TemplateContent);
            if (bookReadToMentorViewModel != null)
            {
                string templateContent = ReadTemplateContent(emailWorkQueue.Template);
                templateContent = templateContent.Replace("[Username]", bookReadToMentorViewModel.ToEmployeeName);

                templateContent = templateContent.Replace("[BookTitle]", bookReadToMentorViewModel.BookTitle);
                templateContent = templateContent.Replace("[AngularUrl]", bookReadToMentorViewModel.AngularURL);

                EmailHelperInputModel emailHelperInputModel = new EmailHelperInputModel()
                {
                    Subject = emailWorkQueue.Subject,
                    Email = bookReadToMentorViewModel.ToEmployeeEmail,
                    Message = templateContent
                };
                SendEmail(emailHelperInputModel);
                return EmailWorkItemStatus.Completed.ToString();
            }

            return EmailWorkItemStatus.Failed.ToString();
        }

        public string ProcessBookReadToCompanyAdmin(EmailWorkQueue emailWorkQueue)
        {
            BookReadToMentorViewModel bookReadToMentorViewModel = new JavaScriptSerializer().Deserialize<BookReadToMentorViewModel>(emailWorkQueue.TemplateContent);
            if (bookReadToMentorViewModel != null)
            {
                string templateContent = ReadTemplateContent(emailWorkQueue.Template);
                templateContent = templateContent.Replace("[Username]", bookReadToMentorViewModel.ToEmployeeName);
                templateContent = templateContent.Replace("[StudentName]", bookReadToMentorViewModel.StudentName);
                templateContent = templateContent.Replace("[BookTitle]", bookReadToMentorViewModel.BookTitle);

                EmailHelperInputModel emailHelperInputModel = new EmailHelperInputModel()
                {
                    Subject = bookReadToMentorViewModel.Subject,
                    Email = bookReadToMentorViewModel.ToEmployeeEmail,
                    Message = templateContent
                };
                SendEmail(emailHelperInputModel);
                return EmailWorkItemStatus.Completed.ToString();
            }

            return EmailWorkItemStatus.Failed.ToString();
        }

        public string ProcessA3CommunicationRequest(EmailWorkQueue emailWorkQueue)
        {
            A3_KaizenCommunicationViewModel output = new JavaScriptSerializer().Deserialize<A3_KaizenCommunicationViewModel>(emailWorkQueue.TemplateContent);
            if (output != null)
            {
                string templateContent = ReadTemplateContent(emailWorkQueue.Template);
                templateContent = templateContent.Replace("[Username]", output.Username);
                templateContent = templateContent.Replace("[URL]", output.URL);
                templateContent = templateContent.Replace("[senderfullname]", output.senderfullname);
                templateContent = templateContent.Replace("[usermessage]", output.usermessage);
                EmailHelperInputModel emailHelperInputModel = new EmailHelperInputModel()
                {
                    Subject = emailWorkQueue.Subject,
                    Email = output.receiveremail,
                    Message = templateContent
                };
                SendEmail(emailHelperInputModel);
                return EmailWorkItemStatus.Completed.ToString();
            }
            return EmailWorkItemStatus.Failed.ToString();
        }

        public string ProcessRenewPlanRequest(EmailWorkQueue emailWorkQueue)
        {
            Task<Employee> taskEmployee = Task.Run(() => _employeeService.GetEmployeeByUserDetailID(emailWorkQueue.SendToEmployee));
            taskEmployee.Wait();
            Employee employee = taskEmployee.Result;
            Task<Subscription> tasksubscription = employee.CompanyId == Guid.Empty ?
     Task.Run(() => _subscriptionservices.GetSubscriptionsByUserDetailIDAndPlanID(emailWorkQueue.SendToEmployee, Convert.ToInt32(emailWorkQueue.KeyID)))
     : Task.Run(() => _subscriptionservices.GetSubscriptionsByCompanyIDAndPlanID(employee.CompanyId, Convert.ToInt32(emailWorkQueue.KeyID)));
            taskEmployee.Wait();
            Subscription subscription = tasksubscription.Result;
            Task<SubscriptionPlan> tasksubscriptionplan = Task.Run(() => _subscriptionservices.GetSubscribedPlan(employee.CompanyId, employee.CompanyId == Guid.Empty ? emailWorkQueue.SendToEmployee : Guid.Empty));
            taskEmployee.Wait();
            SubscriptionPlan subscriptionplan = tasksubscriptionplan.Result;

            if (employee != null)
            {
                string templateContent = ReadTemplateContent(emailWorkQueue.Template);
                templateContent = templateContent.Replace("[Username]", employee.FirstName + " " + employee.LastName);
                templateContent = templateContent.Replace("[PlanName]", subscriptionplan.Name);
                templateContent = templateContent.Replace("[ActivationDate]", string.Format("{0:MMMM dd, yyyy}", subscription.ActivationDate));
                templateContent = templateContent.Replace("[ExpiryDate]", string.Format("{0:MMMM dd, yyyy}", subscription.ExpiryDate));
                templateContent = templateContent.Replace("[TotalNumberOfPlanDays]", Convert.ToString(subscriptionplan.NumberOfDays));
                templateContent = templateContent.Replace("[TotalNumberOfEmployees]", Convert.ToString(subscriptionplan.NumberOfEmployees));
                templateContent = templateContent.Replace("[NumberOfExternalMentors]", Convert.ToString(subscriptionplan.NumberOfExternalMentors));



                EmailHelperInputModel emailHelperInputModel = new EmailHelperInputModel()
                {
                    Subject = emailWorkQueue.Subject,
                    Email = employee.Email,
                    Message = templateContent
                };
                SendEmail(emailHelperInputModel);
                return EmailWorkItemStatus.Completed.ToString();
            }
            return EmailWorkItemStatus.Failed.ToString();
        }

        public string ProcessSubscribePlanRequest(EmailWorkQueue emailWorkQueue)
        {
            Task<Employee> taskEmployee = Task.Run(() => _employeeService.GetEmployeeByUserDetailID(emailWorkQueue.SendToEmployee));
            taskEmployee.Wait();
            Employee employee = taskEmployee.Result;
            Task<Subscription> tasksubscription = employee.CompanyId == Guid.Empty ?
                 Task.Run(() => _subscriptionservices.GetSubscriptionsByUserDetailIDAndPlanID(emailWorkQueue.SendToEmployee, Convert.ToInt32(emailWorkQueue.KeyID)))
                 : Task.Run(() => _subscriptionservices.GetSubscriptionsByCompanyIDAndPlanID(employee.CompanyId, Convert.ToInt32(emailWorkQueue.KeyID)));
            taskEmployee.Wait();
            Subscription subscription = tasksubscription.Result;
            Task<SubscriptionPlan> tasksubscriptionplan = Task.Run(() => _subscriptionservices.GetSubscribedPlan(employee.CompanyId, employee.CompanyId == Guid.Empty ? emailWorkQueue.SendToEmployee : Guid.Empty));
            taskEmployee.Wait();
            SubscriptionPlan subscriptionplan = tasksubscriptionplan.Result;

            if (employee != null)
            {
                string templateContent = ReadTemplateContent(emailWorkQueue.Template);
                templateContent = templateContent.Replace("[Username]", employee.FirstName + " " + employee.LastName);
                templateContent = templateContent.Replace("[PlanName]", subscriptionplan.Name);
                templateContent = templateContent.Replace("[ActivationDate]", string.Format("{0:MMMM dd, yyyy}", subscription.ActivationDate));
                templateContent = templateContent.Replace("[ExpiryDate]", string.Format("{0:MMMM dd, yyyy}", subscription.ExpiryDate));
                templateContent = templateContent.Replace("[TotalNumberOfPlanDays]", Convert.ToString(subscriptionplan.NumberOfDays));
                templateContent = templateContent.Replace("[TotalNumberOfEmployees]", Convert.ToString(subscriptionplan.NumberOfEmployees));
                templateContent = templateContent.Replace("[NumberOfExternalMentors]", Convert.ToString(subscriptionplan.NumberOfExternalMentors));



                EmailHelperInputModel emailHelperInputModel = new EmailHelperInputModel()
                {
                    Subject = emailWorkQueue.Subject,
                    Email = employee.Email,
                    Message = templateContent
                };
                SendEmail(emailHelperInputModel);
                return EmailWorkItemStatus.Completed.ToString();
            }
            return EmailWorkItemStatus.Failed.ToString();
        }


        public string ProcessA3FormUpdateNotifyEmail(EmailWorkQueue emailWorkQueue)
        {
            SendA3FormUpdateNotifyEmail output = new JavaScriptSerializer().Deserialize<SendA3FormUpdateNotifyEmail>(emailWorkQueue.TemplateContent);

            if (output != null)
            {
                string templateContent = ReadTemplateContent(emailWorkQueue.Template);
                templateContent = templateContent.Replace("[Username]", output.receivername);
                templateContent = templateContent.Replace("[FirstName]", output.FirstName);
                templateContent = templateContent.Replace("[LastName]", output.LastName);
                templateContent = templateContent.Replace("[url]", output.url);
                templateContent = templateContent.Replace("[Background]", output.Background);
                templateContent = templateContent.Replace("[CurrentCondition]", output.Condition);
                templateContent = templateContent.Replace("[Analyses]", output.Analyses);
                templateContent = templateContent.Replace("[FollowUp]", output.FollowUp);
                templateContent = templateContent.Replace("[Goal]", output.Goal);
                templateContent = templateContent.Replace("[Plan]", output.Plan);
                templateContent = templateContent.Replace("[Proposal]", output.Proposal);
                templateContent = templateContent.Replace("[DollarImpacted]", output.DollarImpacted);
                templateContent = templateContent.Replace("[Email]", output.Email);



                EmailHelperInputModel emailHelperInputModel = new EmailHelperInputModel()
                {
                    Subject = emailWorkQueue.Subject,
                    Email = output.receiverEmail,
                    Message = templateContent
                };
                SendEmail(emailHelperInputModel);
                return EmailWorkItemStatus.Completed.ToString();
            }
            return EmailWorkItemStatus.Failed.ToString();
        }

        public string ProcessKaizenFormUpdateNotifyEmail(EmailWorkQueue emailWorkQueue)
        {
            SendKaizenFormUpdateNotifyEmail output = new JavaScriptSerializer().Deserialize<SendKaizenFormUpdateNotifyEmail>(emailWorkQueue.TemplateContent);

            if (output != null)
            {
                string templateContent = ReadTemplateContent(emailWorkQueue.Template);
                templateContent = templateContent.Replace("[Username]", output.receivername);
                templateContent = templateContent.Replace("[FirstName]", output.FirstName);
                templateContent = templateContent.Replace("[LastName]", output.LastName);
                templateContent = templateContent.Replace("[url]", output.url);
                templateContent = templateContent.Replace("[DefineTheProblem]", output.DefineTheProblem);
                templateContent = templateContent.Replace("[CurrentCondition]", output.Condition);
                templateContent = templateContent.Replace("[Analyses]", output.Analyses);
                templateContent = templateContent.Replace("[FollowUp]", output.FollowUp);
                templateContent = templateContent.Replace("[Goal]", output.Goal);
                templateContent = templateContent.Replace("[Plan]", output.Plan);
                templateContent = templateContent.Replace("[ActionItemTimeline]", output.ActionItemTimeline);
                templateContent = templateContent.Replace("[DollarImpacted]", output.DollarImpacted);
                templateContent = templateContent.Replace("[Email]", output.Email);



                EmailHelperInputModel emailHelperInputModel = new EmailHelperInputModel()
                {
                    Subject = emailWorkQueue.Subject,
                    Email = output.receiverEmail,
                    Message = templateContent
                };
                SendEmail(emailHelperInputModel);
                return EmailWorkItemStatus.Completed.ToString();
            }
            return EmailWorkItemStatus.Failed.ToString();
        }

        public string ProcessCloseEventMailToAttendee(EmailWorkQueue emailWorkQueue)
        {
            Task<Employee> taskEmployee = Task.Run(() => _employeeService.GetEmployeeByUserDetailID(emailWorkQueue.SendToEmployee));
            taskEmployee.Wait();
            Employee employee = taskEmployee.Result;
            Task<CreateEventInputViewModel> tasktrainingevent = Task.Run(() => _trainingservices.GetEventById(Convert.ToString(emailWorkQueue.KeyID)));
            taskEmployee.Wait();
            CreateEventInputViewModel trainingevent = tasktrainingevent.Result;
            TrainingEventAttendee trainingeventattendee = _trainingservices.GetTrainingEventAttendeeByEmployeeId(Guid.Empty, emailWorkQueue.SendToEmployee);
            taskEmployee.Wait();


            if (employee != null)
            {
                decimal credit = Convert.ToDecimal((trainingeventattendee.Time * trainingeventattendee.Test) / 100);
                string templateContent = ReadTemplateContent(emailWorkQueue.Template);
                templateContent = templateContent.Replace("[Username]", employee.FirstName + " " + employee.LastName);
                templateContent = templateContent.Replace("[TrainingEvent]", trainingevent.eventsname);
                templateContent = templateContent.Replace("[TrainingEventCreator]", employee.FirstName + " " + employee.LastName);
                templateContent = templateContent.Replace("[Credit]", credit.ToString());
                string closenote = trainingevent.ClosingNote;
                templateContent = templateContent.Replace("[ClosingNote]", closenote);


                EmailHelperInputModel emailHelperInputModel = new EmailHelperInputModel()
                {
                    Subject = emailWorkQueue.Subject,
                    Email = employee.Email,
                    Message = templateContent
                };
                SendEmail(emailHelperInputModel);
                return EmailWorkItemStatus.Completed.ToString();
            }
            return EmailWorkItemStatus.Failed.ToString();

        }
        public string ProcessCloseEventEmail(EmailWorkQueue emailWorkQueue)
        {

            CloseEventEmailViewModel closeEventEmailTemplate = new JavaScriptSerializer().Deserialize<CloseEventEmailViewModel>(emailWorkQueue.TemplateContent);

            if (closeEventEmailTemplate != null)
            {
                string templateContent = ReadTemplateContent(emailWorkQueue.Template);
                templateContent = templateContent.Replace("[Username]", closeEventEmailTemplate.FullName);
                templateContent = templateContent.Replace("[TrainingEvent]", closeEventEmailTemplate.TrainingEventName);
                templateContent = templateContent.Replace("[TrainingEventCreator]", closeEventEmailTemplate.TrainingEventCreator);

                EmailHelperInputModel emailHelperInputModel = new EmailHelperInputModel()
                {
                    Subject = emailWorkQueue.Subject,
                    Email = closeEventEmailTemplate.ToEmail,
                    Message = templateContent
                };
                SendEmail(emailHelperInputModel);
                return EmailWorkItemStatus.Completed.ToString();
            }
            return EmailWorkItemStatus.Failed.ToString();

        }
        public string ProcessCloseEventEmailToMentor(EmailWorkQueue emailWorkQueue)
        {

            CloseEventEmailViewModel closeEventEmailTemplate = new JavaScriptSerializer().Deserialize<CloseEventEmailViewModel>(emailWorkQueue.TemplateContent);

            if (closeEventEmailTemplate != null)
            {
                string templateContent = ReadTemplateContent(emailWorkQueue.Template);
                templateContent = templateContent.Replace("[Username]", closeEventEmailTemplate.FullName);
                templateContent = templateContent.Replace("[TrainingEvent]", closeEventEmailTemplate.TrainingEventName);
                templateContent = templateContent.Replace("[TrainingEventCreator]", closeEventEmailTemplate.TrainingEventCreator);

                EmailHelperInputModel emailHelperInputModel = new EmailHelperInputModel()
                {
                    Subject = emailWorkQueue.Subject,
                    Email = closeEventEmailTemplate.ToEmail,
                    Message = templateContent
                };
                SendEmail(emailHelperInputModel);
                return EmailWorkItemStatus.Completed.ToString();
            }
            return EmailWorkItemStatus.Failed.ToString();

        }
        public string ProcessRejectedEmailFromSensei(EmailWorkQueue emailWorkQueue)
        {
            Task<Employee> taskEmployee = Task.Run(() => _employeeService.GetEmployeeByUserDetailID(emailWorkQueue.SendToEmployee));
            taskEmployee.Wait();
            Employee employee = taskEmployee.Result;



            if (employee != null)
            {
                string templateContent = ReadTemplateContent(emailWorkQueue.Template);
                templateContent = templateContent.Replace("[Username]", employee.FirstName + " " + employee.LastName);

                EmailHelperInputModel emailHelperInputModel = new EmailHelperInputModel()
                {
                    Subject = emailWorkQueue.Subject,
                    Email = employee.Email,
                    Message = templateContent
                };
                SendEmail(emailHelperInputModel);
                return EmailWorkItemStatus.Completed.ToString();
            }
            return EmailWorkItemStatus.Failed.ToString();

        }
        public string ProcessTrainingEventInvitationEmail(EmailWorkQueue emailWorkQueue)
        {
            //Task<Employee> taskEmployee = Task.Run(() => _employeeService.GetEmployeeByUserDetailID(emailWorkQueue.SendToEmployee));
            //taskEmployee.Wait();
            //Employee employee = taskEmployee.Result;

            //Task<CreateEventInputViewModel> taskTrainingevent = Task.Run(() => _trainingservices.GetEventById(emailWorkQueue.KeyID));
            //taskEmployee.Wait();
            //CreateEventInputViewModel trainingevent = taskTrainingevent.Result;

            TrainingEventInvitationMailViewModel trainingEventInvitationMail = new JavaScriptSerializer().Deserialize<TrainingEventInvitationMailViewModel>(emailWorkQueue.TemplateContent);

            if (trainingEventInvitationMail != null)
            {
                string templateContent = ReadTemplateContent(emailWorkQueue.Template);
                templateContent = templateContent.Replace("[Username]", trainingEventInvitationMail.FullName);
                templateContent = templateContent.Replace("[TrainingEventCreator]", trainingEventInvitationMail.TrainingEventCreator);
                templateContent = templateContent.Replace("[TrainingEventName]", trainingEventInvitationMail.TrainingEventName);
                templateContent = templateContent.Replace("[Instructor]", trainingEventInvitationMail.Instructor);
                templateContent = templateContent.Replace("[ScheduleDate]", trainingEventInvitationMail.ScheduleDate.ToString());
                templateContent = templateContent.Replace("[TrainingNote]", trainingEventInvitationMail.TrainingNote);

                EmailHelperInputModel emailHelperInputModel = new EmailHelperInputModel()
                {
                    Subject = emailWorkQueue.Subject,
                    Email = trainingEventInvitationMail.EmployeeEmail,
                    Message = templateContent
                };
                SendEmail(emailHelperInputModel);
                return EmailWorkItemStatus.Completed.ToString();
            }
            return EmailWorkItemStatus.Failed.ToString();

        }

        public string ProcessNewEventNotificationEmailToMentor(EmailWorkQueue emailWorkQueue)
        {

            NewEventNotificationViewmodel newEventNotification = new JavaScriptSerializer().Deserialize<NewEventNotificationViewmodel>(emailWorkQueue.TemplateContent);

            if (newEventNotification != null)
            {
                string templateContent = ReadTemplateContent(emailWorkQueue.Template);
                templateContent = templateContent.Replace("[Username]", newEventNotification.ToEmployeeName);
                templateContent = templateContent.Replace("[TrainingEventCreator]", newEventNotification.TrainingEventCreator);
                templateContent = templateContent.Replace("[TrainingEventName]", newEventNotification.TrainingEventName);
                if (newEventNotification.Instructor != "")
                {
                    templateContent = templateContent.Replace("[LblInstructor]", "Instructor");
                    templateContent = templateContent.Replace("[Instructor]", newEventNotification.Instructor);
                }
                else
                {
                    templateContent = templateContent.Replace("[LblInstructor]", "");
                    templateContent = templateContent.Replace("[Instructor]", "");
                }


                templateContent = templateContent.Replace("[ScheduleDate]", newEventNotification.ScheduledDate);
                templateContent = templateContent.Replace("[TrainingNote]", newEventNotification.TrainingNotes);
                templateContent = templateContent.Replace("[StudentsInvited]", newEventNotification.StudentsInvited);

                EmailHelperInputModel emailHelperInputModel = new EmailHelperInputModel()
                {
                    Subject = emailWorkQueue.Subject,
                    Email = newEventNotification.ToEmployeeEmail,
                    Message = templateContent
                };
                SendEmail(emailHelperInputModel);
                return EmailWorkItemStatus.Completed.ToString();
            }
            return EmailWorkItemStatus.Failed.ToString();

        }

        public string ProcessNewEventNotificationEmail(EmailWorkQueue emailWorkQueue)
        {
            NewEventNotificationViewmodel newEventNotificationViewmodel = new JavaScriptSerializer().Deserialize<NewEventNotificationViewmodel>(emailWorkQueue.TemplateContent);
            if (newEventNotificationViewmodel != null)
            {

                string templateContent = ReadTemplateContent(emailWorkQueue.Template);
                templateContent = templateContent.Replace("[Username]", newEventNotificationViewmodel.ToEmployeeName);
                templateContent = templateContent.Replace("[TrainingEventCreator]", newEventNotificationViewmodel.TrainingEventCreator);
                templateContent = templateContent.Replace("[TrainingEventName]", newEventNotificationViewmodel.TrainingEventName);
                templateContent = templateContent.Replace("[Instructor]", newEventNotificationViewmodel.Instructor);
                templateContent = templateContent.Replace("[ScheduleDate]", newEventNotificationViewmodel.ScheduledDate);
                templateContent = templateContent.Replace("[TrainingNote]", newEventNotificationViewmodel.TrainingNotes);
                templateContent = templateContent.Replace("[InvitedStudents]", newEventNotificationViewmodel.StudentsInvited);

                EmailHelperInputModel emailHelperInputModel = new EmailHelperInputModel()
                {
                    Subject = emailWorkQueue.Subject,
                    Email = newEventNotificationViewmodel.ToEmployeeEmail,
                    Message = templateContent
                };
                SendEmail(emailHelperInputModel);
                return EmailWorkItemStatus.Completed.ToString();
            }
            return EmailWorkItemStatus.Failed.ToString();

        }

        private string ReadTemplateContent(string template)
        {
            string templateContent = "";

            string FilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            FilePath = FilePath.Substring(0, FilePath.LastIndexOf("\\") + 1);
            FilePath = FilePath + @"EmailTemplate\" + template;

            StreamReader str = new StreamReader(FilePath);
            templateContent = str.ReadToEnd();
            str.Close();

            return templateContent;
        }

        private void SendEmail(EmailHelperInputModel model)
        {
            string username = ConfigurationManager.AppSettings["FromMailAddress"].ToString();
            string password = ConfigurationManager.AppSettings["FromMailPassword"].ToString();

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(username);

            string[] invalidMails = ConfigurationManager.AppSettings["InvalidMailAdresses"].Split(',');

            if (invalidMails.Contains(model.Email))
            {
                mail.To.Add(ConfigurationManager.AppSettings["AlternateMail"].ToString());
            }

            else
            {
                mail.To.Add(model.Email);
            }
            mail.Subject = model.Subject;
            string Body = model.Message;
            mail.Body = Body;
            mail.IsBodyHtml = true;
            mail.BodyEncoding = Encoding.UTF8;

            SmtpClient smtp = new SmtpClient();
            smtp.Host = ConfigurationManager.AppSettings["SMTPHost"].ToString();
            smtp.Port = Convert.ToInt32(ConfigurationManager.AppSettings["SMTPPort"].ToString());
            smtp.UseDefaultCredentials = true;
            smtp.Credentials = new System.Net.NetworkCredential(username, password);
            smtp.EnableSsl = true;

            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.Send(mail);

        }
        public string ProcessSendUserForSignup(EmailWorkQueue emailWorkQueue)
        {
            Task<Employee> taskEmployee = Task.Run(() => _employeeService.GetEmployeeByUserDetailID(emailWorkQueue.SendToEmployee));
            taskEmployee.Wait();
            Employee employee = taskEmployee.Result;

            if (employee != null)
            {
                string templateContent = ReadTemplateContent(emailWorkQueue.Template);
                templateContent = templateContent.Replace("[Username]", employee.FirstName + " " + employee.LastName);

                EmailHelperInputModel emailHelperInputModel = new EmailHelperInputModel()
                {
                    Subject = emailWorkQueue.Subject,
                    Email = employee.Email,
                    Message = templateContent
                };
                SendEmail(emailHelperInputModel);
                return EmailWorkItemStatus.Completed.ToString();
            }
            return EmailWorkItemStatus.Failed.ToString();
        }
        public string ProcessSendAdminForApproval(EmailWorkQueue emailWorkQueue)
        {
            //Task<Employee> taskEmployee = Task.Run(() => _employeeService.GetEmployeeByUserDetailID(emailWorkQueue.SendToEmployee));
            var SuperAdmin = Task.Run(() => _authenticationService.GetSuperAdminDetail());
            SuperAdmin.Wait();
            UserDetail superAdminDetails = SuperAdmin.Result;

            if (superAdminDetails != null)
            {
                string templateContent = ReadTemplateContent(emailWorkQueue.Template);
                templateContent = templateContent.Replace("[Username]", "Kobus");
                Task<SignupInputViewModel> signupInput = Task.Run(() => _authenticationService.GetCompanyDetailByUserId(emailWorkQueue.SendToEmployee.ToString()));
                signupInput.Wait();
                SignupInputViewModel details = signupInput.Result;
                if (!String.IsNullOrEmpty(details.companyname))
                {
                    templateContent = templateContent.Replace("[FirstNameOrCompayName]", details.FirstName + " is registered himself as company :-" + details.companyname + " and requested for approval.");
                }
                else
                {
                    templateContent = templateContent.Replace("[FirstNameOrCompayName]", details.FirstName + " is requested for Approval.");
                }
                EmailHelperInputModel emailHelperInputModel = new EmailHelperInputModel()
                {
                    Subject = emailWorkQueue.Subject,
                    Email = superAdminDetails.UserName,
                    Message = templateContent
                };
                SendEmail(emailHelperInputModel);
                return EmailWorkItemStatus.Completed.ToString();
            }
            return EmailWorkItemStatus.Failed.ToString();
        }
        public string ProcessQuizAssessmentEmailSendToCompanyAdmin(EmailWorkQueue emailWorkQueue)
        {


            if (emailWorkQueue != null)
            {
                QuizAssMailToAdminViewModel quizAssMailToAdmin = new JavaScriptSerializer().Deserialize<QuizAssMailToAdminViewModel>(emailWorkQueue.TemplateContent);
                string templateContent = ReadTemplateContent(emailWorkQueue.Template);
                templateContent = templateContent.Replace("[Username]", quizAssMailToAdmin.CompanyName);
                templateContent = templateContent.Replace("[EmployeeName]", quizAssMailToAdmin.FullName);
                templateContent = templateContent.Replace("[Credits]", quizAssMailToAdmin.Score.ToString());
                templateContent = templateContent.Replace("[QuezName]", quizAssMailToAdmin.QuizName);

                EmailHelperInputModel emailHelperInputModel = new EmailHelperInputModel()
                {
                    Subject = emailWorkQueue.Subject,
                    Email = quizAssMailToAdmin.CompanyEmail,
                    Message = templateContent
                };
                SendEmail(emailHelperInputModel);
                return EmailWorkItemStatus.Completed.ToString();
            }
            return EmailWorkItemStatus.Failed.ToString();
        }
        public string ProcessQuizAssessmentEmailSendToMentor(EmailWorkQueue emailWorkQueue)
        {
            QuizAssMailToMentorViewModel quizAssMailToMentor = new JavaScriptSerializer().Deserialize<QuizAssMailToMentorViewModel>(emailWorkQueue.TemplateContent);
            if (emailWorkQueue != null)
            {
                string templateContent = ReadTemplateContent(emailWorkQueue.Template);
                templateContent = templateContent.Replace("[Username]", quizAssMailToMentor.MentorFirstName + " " + quizAssMailToMentor.MentorLastName);
                templateContent = templateContent.Replace("[QuizName]", quizAssMailToMentor.QuizName);
                templateContent = templateContent.Replace("[EmployeeName]", quizAssMailToMentor.EmployeeName);
                templateContent = templateContent.Replace("[AngularUrl]", quizAssMailToMentor.AngularUrl);

                EmailHelperInputModel emailHelperInputModel = new EmailHelperInputModel()
                {
                    Subject = emailWorkQueue.Subject,
                    Email = quizAssMailToMentor.MentorEmail,
                    Message = templateContent
                };
                SendEmail(emailHelperInputModel);
                return EmailWorkItemStatus.Completed.ToString();
            }
            return EmailWorkItemStatus.Failed.ToString();
        }

        public string ProcessTechnicalSupportEmail(EmailWorkQueue emailWorkQueue)
        {
            TechnicalSupportEmailViewModel technicalSupportEmailViewModel = new JavaScriptSerializer().Deserialize<TechnicalSupportEmailViewModel>(emailWorkQueue.TemplateContent);
            if (technicalSupportEmailViewModel != null)
            {

                string templateContent = ReadTemplateContent(emailWorkQueue.Template);
                templateContent = templateContent.Replace("[Username]", technicalSupportEmailViewModel.InputName);
                templateContent = templateContent.Replace("[InputName]", technicalSupportEmailViewModel.InputName);
                templateContent = templateContent.Replace("[InputEmail]", technicalSupportEmailViewModel.InputEmail);
                templateContent = templateContent.Replace("[InputPhone]", technicalSupportEmailViewModel.InputPhone);
                templateContent = templateContent.Replace("[InputHelpBox]", technicalSupportEmailViewModel.InputHelpBox);
                templateContent = templateContent.Replace("[InputAdditional]", technicalSupportEmailViewModel.InputAdditional);


                EmailHelperInputModel emailHelperInputModel = new EmailHelperInputModel()
                {
                    Subject = emailWorkQueue.Subject,
                    Email = technicalSupportEmailViewModel.InputEmail,
                    Message = templateContent
                };
                SendEmail(emailHelperInputModel);
                return EmailWorkItemStatus.Completed.ToString();
            }
            return EmailWorkItemStatus.Failed.ToString();

        }

        public string ProcessEmailForDollarApprove(EmailWorkQueue emailWorkQueue)
        {
            EmailForDollarApproveViewModel emailForDollarApproveViewModel = new JavaScriptSerializer().Deserialize<EmailForDollarApproveViewModel>(emailWorkQueue.TemplateContent);
            if (emailForDollarApproveViewModel != null)
            {

                string templateContent = ReadTemplateContent(emailWorkQueue.Template);
                templateContent = templateContent.Replace("[Username]", emailForDollarApproveViewModel.EmployeeName);
                templateContent = templateContent.Replace("[TrainingEventName]", emailForDollarApproveViewModel.TrainingEventName);
                templateContent = templateContent.Replace("[TrainingEventId]", emailForDollarApproveViewModel.TrainingEventId);
                templateContent = templateContent.Replace("[AngularUrl]", emailForDollarApproveViewModel.AngularURL);

                EmailHelperInputModel emailHelperInputModel = new EmailHelperInputModel()
                {
                    Subject = emailWorkQueue.Subject,
                    Email = emailForDollarApproveViewModel.EmployeeEmail,
                    Message = templateContent
                };
                SendEmail(emailHelperInputModel);
                return EmailWorkItemStatus.Completed.ToString();
            }
            return EmailWorkItemStatus.Failed.ToString();

        }
        public string ProcessQuizAssessmentEmailSendToUser(EmailWorkQueue emailWorkQueue)
        {
            QuizAssMailToUserViewModel quizAssMailToUser = new JavaScriptSerializer().Deserialize<QuizAssMailToUserViewModel>(emailWorkQueue.TemplateContent);
            if (emailWorkQueue != null)
            {
                string templateContent = ReadTemplateContent(emailWorkQueue.Template);
                templateContent = templateContent.Replace("[Username]", quizAssMailToUser.Username);
                templateContent = templateContent.Replace("[BookTitle]", quizAssMailToUser.BookTitle);
                templateContent = templateContent.Replace("[Score]", quizAssMailToUser.Score.ToString());

                EmailHelperInputModel emailHelperInputModel = new EmailHelperInputModel()
                {
                    Subject = emailWorkQueue.Subject,
                    Email = quizAssMailToUser.UserEmail,
                    Message = templateContent
                };
                SendEmail(emailHelperInputModel);
                return EmailWorkItemStatus.Completed.ToString();
            }
            return EmailWorkItemStatus.Failed.ToString();
        }
        public string ProcessSendWelcomeEmailToEmployee(EmailWorkQueue emailWorkQueue)
        {
            WelMailToEmpViewModel welMailToEmpViewModel = new JavaScriptSerializer().Deserialize<WelMailToEmpViewModel>(emailWorkQueue.TemplateContent);
            if (emailWorkQueue != null)
            {
                string templateContent = ReadTemplateContent(emailWorkQueue.Template);
                templateContent = templateContent.Replace("[Username]", welMailToEmpViewModel.Username);
                templateContent = templateContent.Replace("[DefaultPass]", ConfigurationManager.AppSettings["DefaultPass"].ToString());

                EmailHelperInputModel emailHelperInputModel = new EmailHelperInputModel()
                {
                    Subject = emailWorkQueue.Subject,
                    Email = welMailToEmpViewModel.UserEmail,
                    Message = templateContent
                };
                SendEmail(emailHelperInputModel);
                return EmailWorkItemStatus.Completed.ToString();
            }
            return EmailWorkItemStatus.Failed.ToString();
        }

        public string ProcessSendChangePasswordEmail(EmailWorkQueue emailWorkQueue)
        {
            ChangePasswordViewModel changePassword = new JavaScriptSerializer().Deserialize<ChangePasswordViewModel>(emailWorkQueue.TemplateContent);
            if (emailWorkQueue != null)
            {
                string templateContent = ReadTemplateContent(emailWorkQueue.Template);
                templateContent = templateContent.Replace("[Username]", changePassword.Username);

                EmailHelperInputModel emailHelperInputModel = new EmailHelperInputModel()
                {
                    Subject = emailWorkQueue.Subject,
                    Email = changePassword.UserEmail,
                    Message = templateContent
                };
                SendEmail(emailHelperInputModel);
                return EmailWorkItemStatus.Completed.ToString();
            }
            return EmailWorkItemStatus.Failed.ToString();
        }
        public string ProcessCloseEventEmailToAttendee(EmailWorkQueue emailWorkQueue)
        {
            CloseEventMailToAttViewModel closeEventMail = new JavaScriptSerializer().Deserialize<CloseEventMailToAttViewModel>(emailWorkQueue.TemplateContent);
            if (emailWorkQueue != null)
            {
                string templateContent = ReadTemplateContent(emailWorkQueue.Template);
                templateContent = templateContent.Replace("[Username]", closeEventMail.FullName);
                templateContent = templateContent.Replace("[TrainingEvent]", closeEventMail.EventName);
                templateContent = templateContent.Replace("[TrainingEventCreator]", closeEventMail.EventCreator);
                templateContent = templateContent.Replace("[Credit]", closeEventMail.Credit.ToString("0.00"));
                templateContent = templateContent.Replace("[ClosingNote]", closeEventMail.ClosingNote);

                EmailHelperInputModel emailHelperInputModel = new EmailHelperInputModel()
                {
                    Subject = emailWorkQueue.Subject,
                    Email = closeEventMail.ToEmail,
                    Message = templateContent
                };
                SendEmail(emailHelperInputModel);
                return EmailWorkItemStatus.Completed.ToString();
            }
            return EmailWorkItemStatus.Failed.ToString();
        }
        public string ProcessCloseA3EventEmailToAttendee(EmailWorkQueue emailWorkQueue)
        {

            CloseA3EvtMailToAttViewModel closeEventEmailTemplate = new JavaScriptSerializer().Deserialize<CloseA3EvtMailToAttViewModel>(emailWorkQueue.TemplateContent);

            if (closeEventEmailTemplate != null)
            {
                string templateContent = ReadTemplateContent(emailWorkQueue.Template);
                templateContent = templateContent.Replace("[Username]", closeEventEmailTemplate.FullName);
                templateContent = templateContent.Replace("[TrainingEvent]", closeEventEmailTemplate.TrainingEventName);
                templateContent = templateContent.Replace("[TrainingEventCreator]", closeEventEmailTemplate.TrainingEventCreator);
                templateContent = templateContent.Replace("[AngularURL]", ConfigurationManager.AppSettings["AngularUrl"].ToString());
                templateContent = templateContent.Replace("[TrainingEventId]", emailWorkQueue.KeyID.ToString());

                EmailHelperInputModel emailHelperInputModel = new EmailHelperInputModel()
                {
                    Subject = emailWorkQueue.Subject,
                    Email = closeEventEmailTemplate.ToEmail,
                    Message = templateContent
                };
                SendEmail(emailHelperInputModel);
                return EmailWorkItemStatus.Completed.ToString();
            }
            return EmailWorkItemStatus.Failed.ToString();

        }

    }
}
