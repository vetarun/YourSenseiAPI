using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourSensei.Data;

namespace YourSensei.WorkQueueProcessor
{
    public interface IEmailHandler
    {
        string ProcessPasswordResetRequest(EmailWorkQueue emailWorkQueue);
        string ProcessBookReadToMentor(EmailWorkQueue emailWorkQueue);
        string ProcessBookReadToCompanyAdmin(EmailWorkQueue emailWorkQueue);
        string ProcessRenewPlanRequest(EmailWorkQueue emailWorkQueue);
        string ProcessSubscribePlanRequest(EmailWorkQueue emailWorkQueue);
        string ProcessCloseEventMailToAttendee(EmailWorkQueue emailWorkQueue);
        string ProcessCloseEventEmail(EmailWorkQueue emailWorkQueue);
        string ProcessSendUserForSignup(EmailWorkQueue emailWorkQueue);
        string ProcessSendAdminForApproval(EmailWorkQueue emailWorkQueue);
        string ProcessRejectedEmailFromSensei(EmailWorkQueue emailWorkQueue);
        string ProcessTrainingEventInvitationEmail(EmailWorkQueue emailWorkQueue);
        string ProcessNewEventNotificationEmailToMentor(EmailWorkQueue emailWorkQueue);
        string ProcessA3CommunicationRequest(EmailWorkQueue emailWorkQueue);
        string ProcessQuizAssessmentEmailSendToCompanyAdmin(EmailWorkQueue emailWorkQueue);
        string ProcessQuizAssessmentEmailSendToMentor(EmailWorkQueue emailWorkQueue);
        //string ProcessQuizAssessmentEmailSendToCompanyAdmin(EmailWorkQueue emailWorkQueue);
        string ProcessNewEventNotificationEmail(EmailWorkQueue emailWorkQueue);
        string ProcessA3FormUpdateNotifyEmail(EmailWorkQueue emailWorkQueue);
        string ProcessKaizenFormUpdateNotifyEmail(EmailWorkQueue emailWorkQueue);
        string ProcessTechnicalSupportEmail(EmailWorkQueue emailWorkQueue);
        string ProcessEmailForDollarApprove(EmailWorkQueue emailWorkQueue);
        string ProcessQuizAssessmentEmailSendToUser(EmailWorkQueue emailWorkQueue);
        string ProcessSendWelcomeEmailToEmployee(EmailWorkQueue emailWorkQueue);
        string ProcessSendChangePasswordEmail(EmailWorkQueue emailWorkQueue);
        string ProcessBookReadToEmployee(EmailWorkQueue emailWorkQueue);
        string ProcessCloseEventEmailToAttendee(EmailWorkQueue emailWorkQueue);
        string ProcessCloseA3EventEmailToAttendee(EmailWorkQueue emailWorkQueue);
    }
}
