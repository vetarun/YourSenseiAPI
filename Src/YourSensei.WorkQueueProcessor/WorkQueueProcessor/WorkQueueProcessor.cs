using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourSensei.Data;
using YourSensei.Service;
using YourSensei.Utility;
using YourSensei.ViewModel;
using YourSensei.ViewModel.Dashboard;
using YourSensei.WorkQueueProcessor.BeltAchievementHandler;

namespace YourSensei.WorkQueueProcessor
{
    public class WorkQueueProcessor : IWorkQueueProcessor
    {
        private readonly IWorkQueueService _workQueueService;
        private readonly ISubscriptionHandler _subscriptionHandler;
        private readonly IBeltRulesHandler _beltRulesHandler;
        private readonly IDashboardService _dashboard;
        private readonly IEmployeeService _employee;
        private readonly IWeeklyEmailUpdate _email;
        private readonly IEmailWorkQueueService _emailWorkQueueService;
        private readonly IEmailHandler _emailHandler;

        public WorkQueueProcessor(WorkQueueService workQueueService, SubscriptionHandler subscriptionHandler, BeltRulesHandler beltRulesHandler,
            DashboardService dashboard, EmployeeService employee, WeeklyEmailUpdate email, EmailWorkQueueService emailWorkQueueService,
            EmailHandler emailHandler)
        {
            _workQueueService = workQueueService;
            _subscriptionHandler = subscriptionHandler;
            _beltRulesHandler = beltRulesHandler;
            _dashboard = dashboard;
            _employee = employee;
            _email = email;
            _emailWorkQueueService = emailWorkQueueService;
            _emailHandler = emailHandler;
        }

        public void ProcessDailyAt12AM()
        {
            try
            {
                Console.WriteLine("Achievement start");
                _beltRulesHandler.ProcessBeltAchievement();
                Console.WriteLine("Achievement end");
                Task<List<WorkQueue>> taskWorkQueueList = Task.Run(() => _workQueueService.GetWorkQueueByStatus(WorkItemStatus.Pending.ToString()));
                taskWorkQueueList.Wait();
                List<WorkQueue> workQueues = taskWorkQueueList.Result;

                foreach (WorkQueue workQueue in workQueues)
                {
                    switch (workQueue.WorkItemType)
                    {
                        case "RenewSubscription":
                            Console.WriteLine("RenewSubscription start");
                            workQueue.Status = WorkItemStatus.InProgress.ToString();
                            workQueue.ModifiedDate = DateTime.UtcNow;
                            _workQueueService.Save(workQueue);

                            string workQueueStatus = _subscriptionHandler.ProcessRenewRequest(workQueue);
                            workQueue.Status = workQueueStatus;
                            workQueue.ModifiedDate = DateTime.UtcNow;
                            _workQueueService.Save(workQueue);
                            Console.WriteLine("RenewSubscription end");
                            break;
                        case "DeleteBeltRule":
                            Console.WriteLine("DeleteBeltRule start");
                            workQueue.Status = WorkItemStatus.InProgress.ToString();
                            workQueue.ModifiedDate = DateTime.UtcNow;
                            _workQueueService.Save(workQueue);

                            string wqStatus = _beltRulesHandler.deleteBeltRules(workQueue).ToString();
                            workQueue.Status = wqStatus;
                            workQueue.ModifiedDate = DateTime.UtcNow;
                            _workQueueService.Save(workQueue);
                            Console.WriteLine("DeleteBeltRule end");
                            break;
                        default:
                            break;
                    }
                }

                Console.WriteLine("ProcessingExpirydate start");
                _subscriptionHandler.ProcessingExpirydate();
                Console.WriteLine("ProcessingExpirydate end");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void WeeklyEmailUpdate()
        {
            try
            {
                string userid = string.Empty;
                var ListOfEmp = _email.GetAllEmployee();
                foreach (var item in ListOfEmp)
                {

                    if (item.RoleId == "Company User" || item.RoleId == "Company Admin" || item.RoleId == "Individual")
                    {
                        Task<GetDashboardBeltRuleViewModel> BeltDetails = Task.Run(() => _dashboard.GetDashboardBeltDetails(item.CompanyId, item.userDetialID, item.Id, false));
                        BeltDetails.Wait();
                        GetDashboardBeltRuleViewModel beltdetails = BeltDetails.Result;
                        userid = item.UserCategory.ToUpper() == "FBDE320E-6619-4F25-9E7F-2FCC94D2879E".ToUpper() ? item.userDetialID : "00000000-0000-0000-0000-000000000000";
                        Task<List<CreditStandingViewModel>> GetCreditStandings = Task.Run(() => _dashboard.GetCreditStandings(item.CompanyId, userid, false));
                        GetCreditStandings.Wait();
                        List<CreditStandingViewModel> getCreditStandings = GetCreditStandings.Result;


                        _email.SendWeeklyEmail(beltdetails, getCreditStandings, item);

                    }

                }

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public void ProcessEmailEvery5Minutes()
        {
            try
            {
                Task<List<EmailWorkQueue>> taskEmailWorkQueueList = Task.Run(() => _emailWorkQueueService.GetEmailWorkQueueByStatus(EmailWorkItemStatus.Pending.ToString()));
                taskEmailWorkQueueList.Wait();
                List<EmailWorkQueue> emailWorkQueues = taskEmailWorkQueueList.Result;

                foreach (EmailWorkQueue emailWorkQueue in emailWorkQueues)
                {
                    switch (emailWorkQueue.WorkItemType)
                    {
                        case "PasswordReset":
                            emailWorkQueue.Status = EmailWorkItemStatus.InProgress.ToString();
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            Task<ResponseViewModel> taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();

                            string workQueueStatus = _emailHandler.ProcessPasswordResetRequest(emailWorkQueue);
                            emailWorkQueue.Status = workQueueStatus;
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();
                            break;
                        case "RenewPlan":
                            emailWorkQueue.Status = EmailWorkItemStatus.InProgress.ToString();
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();

                            string renewplanStatus = _emailHandler.ProcessRenewPlanRequest(emailWorkQueue);
                            emailWorkQueue.Status = renewplanStatus;
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();
                            break;
                        case "SubscribePlan":
                            emailWorkQueue.Status = EmailWorkItemStatus.InProgress.ToString();
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();

                            string SubscribePlanStatus = _emailHandler.ProcessSubscribePlanRequest(emailWorkQueue);
                            emailWorkQueue.Status = SubscribePlanStatus;
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();
                            break;
                        case "SendA3FormUpdateNotifyEmail":
                            emailWorkQueue.Status = EmailWorkItemStatus.InProgress.ToString();
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();

                            string A3FormUpdateNotifyEmail = _emailHandler.ProcessA3FormUpdateNotifyEmail(emailWorkQueue);
                            emailWorkQueue.Status = A3FormUpdateNotifyEmail;
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();
                            break;
                        case "SendKaizenFormUpdateNotifyEmail":
                            emailWorkQueue.Status = EmailWorkItemStatus.InProgress.ToString();
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();

                            string KaizenFormUpdateNotifyEmail = _emailHandler.ProcessKaizenFormUpdateNotifyEmail(emailWorkQueue);
                            emailWorkQueue.Status = KaizenFormUpdateNotifyEmail;
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();
                            break;

                        case "A3Communication":
                            emailWorkQueue.Status = EmailWorkItemStatus.InProgress.ToString();
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();

                            string A3CommunicationStatus = _emailHandler.ProcessA3CommunicationRequest(emailWorkQueue);
                            emailWorkQueue.Status = A3CommunicationStatus;
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();
                            break;
                        case "CloseEventEmail":
                            emailWorkQueue.Status = EmailWorkItemStatus.InProgress.ToString();
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();

                            string closeEventMail = _emailHandler.ProcessCloseEventEmail(emailWorkQueue);
                            emailWorkQueue.Status = closeEventMail;
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();
                            break;
                        case "CloseEventEmailToMentor":
                            emailWorkQueue.Status = EmailWorkItemStatus.InProgress.ToString();
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();

                            string closeEventMailToMentor = _emailHandler.ProcessCloseEventEmailToMentor(emailWorkQueue);
                            emailWorkQueue.Status = closeEventMailToMentor;
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();
                            break;
                        case "RejectedEmailFromSensei":
                            emailWorkQueue.Status = EmailWorkItemStatus.InProgress.ToString();
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();

                            string rejectedEmailFromSensei = _emailHandler.ProcessRejectedEmailFromSensei(emailWorkQueue);
                            emailWorkQueue.Status = rejectedEmailFromSensei;
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();
                            break;
                        case "SendUserForSignup":
                            emailWorkQueue.Status = EmailWorkItemStatus.InProgress.ToString();
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();

                            string SendUserForSignup = _emailHandler.ProcessSendUserForSignup(emailWorkQueue);
                            emailWorkQueue.Status = SendUserForSignup;
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();
                            break;
                        case "TrainingEventInvitationEmail":
                            emailWorkQueue.Status = EmailWorkItemStatus.InProgress.ToString();
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();

                            string trainingEventInvitationEmail = _emailHandler.ProcessTrainingEventInvitationEmail(emailWorkQueue);
                            emailWorkQueue.Status = trainingEventInvitationEmail;
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();
                            break;
                        case "NewEventNotificationEmailToMentor":
                            emailWorkQueue.Status = EmailWorkItemStatus.InProgress.ToString();
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();

                            string NewEventNotificationEmailToMentor = _emailHandler.ProcessNewEventNotificationEmailToMentor(emailWorkQueue);
                            emailWorkQueue.Status = NewEventNotificationEmailToMentor;
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();
                            break;
                        case "SendAdminForApproval":
                            emailWorkQueue.Status = EmailWorkItemStatus.InProgress.ToString();
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();

                            string SendAdminForApproval = _emailHandler.ProcessSendAdminForApproval(emailWorkQueue);
                            emailWorkQueue.Status = SendAdminForApproval;
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();
                            break;
                        case "BookReadToMentor":
                            emailWorkQueue.Status = EmailWorkItemStatus.InProgress.ToString();
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();

                            string BookReadToMentor = _emailHandler.ProcessBookReadToMentor(emailWorkQueue);
                            emailWorkQueue.Status = BookReadToMentor;
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();
                            break;

                        case "BookReadToEmployee":
                        case "BookReadWithQuizToEmployee":
                            emailWorkQueue.Status = EmailWorkItemStatus.InProgress.ToString();
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();

                            string BookReadToEmployee = _emailHandler.ProcessBookReadToEmployee(emailWorkQueue);
                            emailWorkQueue.Status = BookReadToEmployee;
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();
                            break;
                        case "QuizAssessmentEmailSendToCompanyAdmin":
                            emailWorkQueue.Status = EmailWorkItemStatus.InProgress.ToString();
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();

                            string QuizAssessmentEmailSendToCompanyAdmin = _emailHandler.ProcessQuizAssessmentEmailSendToCompanyAdmin(emailWorkQueue);
                            emailWorkQueue.Status = QuizAssessmentEmailSendToCompanyAdmin;
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();
                            break;
                        case "QuizAssessmentEmailSendToMentor":
                            emailWorkQueue.Status = EmailWorkItemStatus.InProgress.ToString();
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();

                            string QuizAssessmentEmailSendToMentor = _emailHandler.ProcessQuizAssessmentEmailSendToMentor(emailWorkQueue);
                            emailWorkQueue.Status = QuizAssessmentEmailSendToMentor;
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();
                            break;
                        case "BookReadToCompanyAdmin":
                            emailWorkQueue.Status = EmailWorkItemStatus.InProgress.ToString();
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();

                            string BookReadToCompanyAdmin = _emailHandler.ProcessBookReadToCompanyAdmin(emailWorkQueue);
                            emailWorkQueue.Status = BookReadToCompanyAdmin;
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();
                            break;
                        default:
                            break;
                        case "QuizAssessmentEmailSendToUser":
                            emailWorkQueue.Status = EmailWorkItemStatus.InProgress.ToString();
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();

                            string QuizAssessmentEmailSendToUser = _emailHandler.ProcessQuizAssessmentEmailSendToUser(emailWorkQueue);
                            emailWorkQueue.Status = QuizAssessmentEmailSendToUser;
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();
                            break;

                        case "SendWelcomeEmailToEmployee":
                            emailWorkQueue.Status = EmailWorkItemStatus.InProgress.ToString();
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();

                            string SendWelcomeEmailToEmployee = _emailHandler.ProcessSendWelcomeEmailToEmployee(emailWorkQueue);
                            emailWorkQueue.Status = SendWelcomeEmailToEmployee;
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();
                            break;
                        case "SendChangePasswordEmail":
                            emailWorkQueue.Status = EmailWorkItemStatus.InProgress.ToString();
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();

                            string SendChangePasswordEmail = _emailHandler.ProcessSendChangePasswordEmail(emailWorkQueue);
                            emailWorkQueue.Status = SendChangePasswordEmail;
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();
                            break;



                        case "NewEventNotificationEmail":
                            emailWorkQueue.Status = EmailWorkItemStatus.InProgress.ToString();
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            _emailWorkQueueService.Save(emailWorkQueue);

                            string newEventNotificationEmail = _emailHandler.ProcessNewEventNotificationEmail(emailWorkQueue);
                            emailWorkQueue.Status = newEventNotificationEmail;
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            _emailWorkQueueService.Save(emailWorkQueue);
                            break;

                        case "TechnicalSupportEmail":
                            emailWorkQueue.Status = EmailWorkItemStatus.InProgress.ToString();
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;

                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();

                            string technicalSupportEmail = _emailHandler.ProcessTechnicalSupportEmail(emailWorkQueue);
                            emailWorkQueue.Status = technicalSupportEmail;
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();
                            break;

                        case "EmailForDollarApprove":
                            emailWorkQueue.Status = EmailWorkItemStatus.InProgress.ToString();
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();

                            string emailForDollarApprove = _emailHandler.ProcessEmailForDollarApprove(emailWorkQueue);
                            emailWorkQueue.Status = emailForDollarApprove;
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();
                            break;

                        case "CloseEventEmailToAttendee":
                            emailWorkQueue.Status = EmailWorkItemStatus.InProgress.ToString();
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();

                            string closeEventEmailToAttendee = _emailHandler.ProcessCloseEventEmailToAttendee(emailWorkQueue);
                            emailWorkQueue.Status = closeEventEmailToAttendee;
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();
                            break;

                        case "CloseA3EventEmailToAttendee":
                            emailWorkQueue.Status = EmailWorkItemStatus.InProgress.ToString();
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();

                            string CloseA3EventEmailToAttendee = _emailHandler.ProcessCloseA3EventEmailToAttendee(emailWorkQueue);
                            emailWorkQueue.Status = CloseA3EventEmailToAttendee;
                            emailWorkQueue.ModifiedDate = DateTime.UtcNow;
                            taskResponseViewModel = Task.Run(() => _emailWorkQueueService.Save(emailWorkQueue));
                            taskResponseViewModel.Wait();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
