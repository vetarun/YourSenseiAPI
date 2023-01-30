using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using YourSensei.Data;
using YourSensei.Utility;
using YourSensei.ViewModel;
using YourSensei.ViewModel.Dashboard;

namespace YourSensei.Utility
{
    public static class SendMail
    {
        private static string AngularUrl = ConfigurationManager.AppSettings["AngularUrl"].ToString();

        public static string sendEmail(EmailHelperInputModel model)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(ConfigurationManager.AppSettings["FromMailAddress"].ToString());
                mail.To.Add(model.Email);

                //todo dynamic subject
                mail.Subject = model.Subject;
                string Body = model.Message;
                mail.Body = Body;
                mail.IsBodyHtml = true;
                mail.BodyEncoding = System.Text.Encoding.UTF8;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = ConfigurationManager.AppSettings["SMTPHost"].ToString();
                smtp.Port = Convert.ToInt32(ConfigurationManager.AppSettings["SMTPPort"].ToString());
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["FromMailAddress"].ToString(), ConfigurationManager.AppSettings["FromMailPassword"].ToString());// Enter senders Email and password
                smtp.EnableSsl = true;

                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Send(mail);
                return "Email Sent";
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        public static string SendPasswordResetEmail(string email)
        {

            DateTime expreddate = System.DateTime.Now.AddHours(48.0);



            string datatoencrypt = "email:" + email + "&expiredate:" + expreddate;
            string encryptdata = Encryption.Encrypt(datatoencrypt, "123456789");
            string encryptdata1 = Encryption.Decrypt(encryptdata, "123456789");

            EmailHelperInputModel message = new EmailHelperInputModel();
            message.Subject = "Resest Password";
            message.Email = email;
            string FilePath = HttpContext.Current.Server.MapPath("~/");
            // FilePath = FilePath.Replace("YourSensei\\", "");
            //  FilePath = FilePath + @"Src\YourSensei.Utility\SendMail\EmailTemplate\Notification.html";
            FilePath = FilePath.Substring(0, FilePath.LastIndexOf("\\") + 1);
            FilePath = FilePath + @"bin\EmailTemplate\Notification.html";
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();

            MailText = MailText.Replace("[newusername]", " your sensei user,");
            MailText = MailText.Replace("[content]", "Please click on this link to reset your password <a href =http://www.yoursensei.org/#/Main/resetpassword?resetlink="
              + encryptdata + ">Reset Password</a>");
            //StringBuilder str = new StringBuilder();
            //str.Append(string.Format("{0},{1}", @"<p><strong>Dear your sensei user,</strong>", ""));
            //str.Append($"<p>Please click on this link to reset your password <a href =http://www.yoursensei.org/#/Main/resetpassword?resetlink=" + encryptdata + ">Reset Password</a> </p></br>");
            //str.Append(@"<p><strong>Thanks</strong></p>");

            message.Message = MailText;
            var sendEmailResponse = sendEmail(message);
            return sendEmailResponse;
        }

        public static string SendCloseEventEmailToAttendee(TrainingEvent trainingEvent, EmployeeResponseViewModel toEmloyee, Employee trainingEventCreator, decimal credit)
        {

            EmailHelperInputModel message = new EmailHelperInputModel();
            message.Subject = "Event closed - " + trainingEvent.Name;
            message.Email = toEmloyee.Email;
            //message.Email = "tarunarora@virtualemployee.com";

            string FilePath = HttpContext.Current.Server.MapPath("~/");
            //FilePath = FilePath.Replace("YourSensei\\", "");
            //FilePath = FilePath + @"Src\YourSensei.Utility\SendMail\EmailTemplate\Notification.html";
            FilePath = FilePath.Substring(0, FilePath.LastIndexOf("\\") + 1);
            FilePath = FilePath + @"bin\EmailTemplate\Notification.html";
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();

            MailText = MailText.Replace("[newusername]", toEmloyee.FirstName + " " + toEmloyee.LastName);
            MailText = MailText.Replace("[content]", "The " + trainingEvent.Name + " training event has been closed by " + trainingEventCreator.FirstName + " " +
                trainingEventCreator.LastName + "<br> <br>" +
                "You have earned " + credit.ToString("0.00") + " credit points for this event." + "<br><br>" +
                "Closing Notes " + trainingEvent.ClosingNote);
            //StringBuilder str = new StringBuilder();
            //str.AppendLine("<p><strong>Dear " + toEmloyee.FirstName + " " + toEmloyee.LastName + "</strong>,</p><br />");
            //str.AppendLine("<p>The <strong>" + trainingEvent.Name + "</strong> training event has been closed by " + trainingEventCreator.FirstName + " " +
            //    trainingEventCreator.LastName + "</p>");

            //str.AppendLine("<p>You have earned <strong>" + credit.ToString("0.00") + "</strong> credit points for this event.</p><br />");
            //str.AppendLine("<p><strong>Closing Notes</strong>: " + trainingEvent.ClosingNote + "</p><br />");
            //str.AppendLine("Warm Regards,<br />");
            //str.AppendLine("Your Sensei<br />");

            message.Message = MailText;
            var sendEmailResponse = sendEmail(message);
            return sendEmailResponse;
        }

        public static string SendCloseEventEmail(List<EmployeeResponseViewModel> empList, Employee toEmployee, TrainingEvent trainingEvent, Employee trainingEventCreator, bool isA3Event)
        {
            EmailHelperInputModel message = new EmailHelperInputModel();
            message.Subject = isA3Event ? "Event Submitted - " + trainingEvent.Name : "Event closed - " + trainingEvent.Name;
            message.Email = toEmployee.Email;
            //message.Email = "tarunarora@virtualemployee.com";
            string FilePath = HttpContext.Current.Server.MapPath("~/");
            FilePath = FilePath.Substring(0, FilePath.LastIndexOf("\\") + 1);
            FilePath = FilePath + @"bin\EmailTemplate\Notification.html";
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();

            MailText = MailText.Replace("[newusername]", toEmployee.FirstName + " " + toEmployee.LastName);

            if (isA3Event)
            {
                MailText = MailText.Replace("[content]", "The " + trainingEvent.Name + " training event has been closed by " + trainingEventCreator.FirstName + " " + trainingEventCreator.LastName + "<br><br>Please click <strong><a href =\"" + AngularUrl + "/Main/studentevent?id=" + trainingEvent.ID + "\">here</a></strong> to review this Event.<br>");
            }
            else
            {
                MailText = MailText.Replace("[content]", "The " + trainingEvent.Name + " training event has been closed by " + trainingEventCreator.FirstName + " " + trainingEventCreator.LastName + "");
            }
            //StringBuilder str = new StringBuilder();
            //str.AppendLine("<p><strong>Dear " + toEmployee.FirstName + " " + toEmployee.LastName + "</strong>,</p><br />");
            //str.AppendLine("<p>The <strong>" + trainingEvent.Name + "</strong> training event has been closed by " + trainingEventCreator.FirstName + " " +
            //    trainingEventCreator.LastName + "</p><br />");
            //if (isA3Event)
            //{
            //    str.AppendLine("Please click <strong><a href=\"" + AngularUrl + "/Main/studentevent?id=" + trainingEvent.ID + "\">here</a></strong> to review the A3 Event.<br />");

            //}
            //str.AppendLine("Students involved in this event:");
            //str.AppendLine(GetEmpTableWithCredit(empList).ToString() + "<br />");
            //str.AppendLine("<p><strong>Closing Notes</strong>: " + trainingEvent.ClosingNote + "</p><br />");

            //str.AppendLine("Warm Regards,<br />");
            //str.AppendLine("Your Sensei<br />");

            message.Message = MailText;
            var sendEmailResponse = sendEmail(message);
            return sendEmailResponse;
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

        public static StringBuilder GetEmpTableWithCredit(List<EmployeeResponseViewModel> emp)
        {
            StringBuilder orderStr = new StringBuilder();

            orderStr.Append("<table style=\"font-size: 10pt;font-family: Verdana, Arial, Helvetica, sans-serif;border: 1px solid #ccc;text-align: left;\" cellspacing=\"0\" cellpadding=\"10\">");
            orderStr.Append("<tr><th style=\"text-align:left\">Employee code</th><th style=\"text-align:left\">Employee Name</th><th style=\"text-align:left\">Email</th><th style=\"text-align:left\">Credit</th></tr>");
            foreach (var item in emp)
            {
                orderStr.Append("<tr>" +
               "<td style=\"text-align:left\">" + item.EmployeeCode + "</td>" +
               "<td style=\"text-align:left\">" + item.FirstName + " " + item.LastName + "</td>" +
               "<td style=\"text-align:left\">" + (item.Email) + "</td>" +
               "<td style=\"text-align:left\">" + (item.Credit.ToString("0.00")) + "</td>" +
                "</tr>");

            }
            orderStr.Append("</table>");
            return orderStr;
        }

        public static string BookReadEmailSendToMentor(Employee employee, CompanyLibraryBook book, string EmployeeName)
        {
            EmailHelperInputModel message = new EmailHelperInputModel();
            message.Subject = "Book Read -" + book.Title;
            message.Email = employee.Email;
            //message.Email = "virtualemployee3145@gmail.com";

            // string FilePath = "D:\\Saurabh\\YourSensei Project\\YourSenseiWebAPI\\Src\\YourSensei.Utility\\SendMail\\EmailTemplate\\Notification.html";
            string FilePath = HttpContext.Current.Server.MapPath("~/");
            // FilePath = FilePath.Replace("YourSensei\\", "");
            //FilePath = FilePath + @"Src\YourSensei.Utility\SendMail\EmailTemplate\Notification.html";
            FilePath = FilePath.Substring(0, FilePath.LastIndexOf("\\") + 1);
            FilePath = FilePath + @"bin\EmailTemplate\Notification.html";
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();

            //Repalce [newusername] = signup user name   
            MailText = MailText.Replace("[newusername]", employee.FirstName + " " + employee.LastName);
            MailText = MailText.Replace("[content]", "Good news: Your student < strong >" + EmployeeName + "</ strong > is making progress!This is to notify you that your" +
                "student completed the <strong>" + book.Title + "</strong>. This is a good opportunity for you to congratulate your student and" +
                "to think about any practical way you can help your student put their new knowledge to the test (with a face-to-face discussion" +
                "about the deeper concepts in this book, an A3 diagram or Kaizen event or suggesting a book that will build on their new knowledge).<br><br>Please click<strong> < a href =\"" + AngularUrl + "Main/login\">here</a></strong> to review the progress of your studens.<br>");
            //StringBuilder str = new StringBuilder();
            //str.AppendLine(string.Format("<p><strong>Dear " + employee.FirstName + " " + employee.LastName + "</strong>,</p><br />"));
            //str.AppendLine("<p>Good news: Your student <strong>" + EmployeeName + "</strong> is making progress! This is to notify you that your " +
            //    "student completed the <strong>" + book.Title + "</strong>. This is a good opportunity for you to congratulate your student and " +
            //    "to think about any practical way you can help your student put their new knowledge to the test (with a face-to-face discussion " +
            //    "about the deeper concepts in this book, an A3 diagram or Kaizen event or suggesting a book that will build on their new knowledge).</p><br />");
            //str.AppendLine("Please click <strong><a href=\"" + AngularUrl + "Main/login\">here</a></strong> to review the progress of your studens.<br />");
            //str.AppendLine("Warm Regards,<br />");
            //str.AppendLine("Your Sensei");

            message.Message = MailText;
            var sendEmailResponse = sendEmail(message);
            return sendEmailResponse;


        }
        public static string BookReadEmailSendToCompanyAdmin(CompanyDetail CompAdminEmail, CompanyLibraryBook book, string EmployeeName)
        {
            EmailHelperInputModel message = new EmailHelperInputModel();
            message.Subject = "Book Read-" + book.Title;
            message.Email = CompAdminEmail.email;
            //message.Email = "virtualemployee3145@gmail.com";
            //  string FilePath = "D:\\Saurabh\\YourSensei Project\\YourSenseiWebAPI\\Src\\YourSensei.Utility\\SendMail\\EmailTemplate\\Notification.html";
            string FilePath = HttpContext.Current.Server.MapPath("~/");
            //  FilePath = FilePath.Replace("YourSensei\\", "");
            // FilePath = FilePath + @"Src\YourSensei.Utility\SendMail\EmailTemplate\Notification.html";
            FilePath = FilePath.Substring(0, FilePath.LastIndexOf("\\") + 1);
            FilePath = FilePath + @"bin\EmailTemplate\Notification.html";
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();

            //Repalce [newusername] = signup user name   
            MailText = MailText.Replace("[newusername]", EmployeeName);
            MailText = MailText.Replace("[content]", "The " + book.Title + " Book has been read by " + EmployeeName);
            //StringBuilder str = new StringBuilder();
            //str.AppendLine("<p><strong>Dear " + CompAdminEmail.companyname + "</strong>,</p><br />");
            //str.Append(@"<p> The " + book.Title + " Book has been read by " + EmployeeName + "</p>");
            //str.AppendLine("Warm Regards,<br />");
            //str.AppendLine("Your Sensei<br />");

            message.Message = MailText;
            var sendEmailResponse = sendEmail(message);
            return sendEmailResponse;


        }

        public static string BookReadEmailSendToEmployee(CompanyLibraryBook book, Employee emp)
        {
            EmailHelperInputModel message = new EmailHelperInputModel();
            message.Subject = "Book Read- " + book.Title;
            message.Email = emp.Email;
            //message.Email = "virtualemployee3145@gmail.com";
            //string FilePath = "D:\\Saurabh\\YourSensei Project\\YourSenseiWebAPI\\Src\\YourSensei.Utility\\SendMail\\EmailTemplate\\Notification.html";
            string FilePath = HttpContext.Current.Server.MapPath("~/");
            // FilePath = FilePath.Replace("YourSensei\\", "");
            //FilePath = FilePath + @"Src\YourSensei.Utility\SendMail\EmailTemplate\Notification.html";
            FilePath = FilePath.Substring(0, FilePath.LastIndexOf("\\") + 1);
            FilePath = FilePath + @"bin\EmailTemplate\Notification.html";
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();

            MailText = MailText.Replace("[newusername]", emp.FirstName + " " + emp.LastName);
            if (book.QuizID == 0)
            {
                MailText = MailText.Replace("[content]", "Well done on completing " + book.Title + " and thank you for completing the book rating");
            }
            else
            {
                MailText = MailText.Replace("[content]", "Well done on completing " + book.Title + "and thank you for completing the book rating. You are now ready to" +
                    "complete the quiz if you did not complete it already. Click <a  href=\"" + AngularUrl + "Main/library\">here</a>"
                    + "to complete the quiz.");
            }
            //StringBuilder str = new StringBuilder();
            //str.AppendLine("<p><strong>Dear " + emp.FirstName + " " + emp.LastName + "</strong>,</p><br />");
            //if (book.QuizID == 0)
            //{
            //    str.AppendLine("<p>Well done on completing " + book.Title + " and thank you for completing the book rating.</p><br />");
            //}
            //else
            //{
            //    str.AppendLine("<p>Well done on completing " + book.Title + " and thank you for completing the book rating. You are now ready to " +
            //        "complete the quiz if you did not complete it already. Click <strong><a href=\"" + AngularUrl + "Main/library\">here</a></strong> " +
            //        "to complete the quiz.</p><br />");
            //}
            //str.AppendLine("Warm Regards,<br />");
            //str.AppendLine("Your Sensei<br />");

            message.Message = MailText;
            var sendEmailResponse = sendEmail(message);
            return sendEmailResponse;


        }
        public static string SendUserForSignup(SignupInputViewModel details)
        {
            EmailHelperInputModel message = new EmailHelperInputModel();
            message.Subject = "Sign Up Successful. Welcome to YourSensei!";
            message.Email = details.Email;
            //message.Email = "virtualemployee3145@gmail.com";
            string FilePath = HttpContext.Current.Server.MapPath("~/");
            // FilePath = FilePath.Replace("YourSensei\\", "");
            //FilePath = FilePath + @"Src\YourSensei.Utility\SendMail\EmailTemplate\Notification.html";
            FilePath = FilePath.Substring(0, FilePath.LastIndexOf("\\") + 1);
            FilePath = FilePath + @"bin\EmailTemplate\Notification.html";
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();

            MailText = MailText.Replace("[newusername]", details.FirstName + "" + details.LastName);
            MailText = MailText.Replace("[content]", " Thank you for registering your company with YourSensei. We are looking forward to see your company and its people grow" +
                "stronger in the months and years ahead.");

            //StringBuilder str = new StringBuilder();
            //str.Append(string.Format("{0},{1}", @"<p><strong>Dear " + details.FirstName + "</strong>", ""));

            //str.Append("<p>Thank you for registering your company with YourSensei. We are looking forward to see your company and its people grow " + 
            //    "stronger in the months and years ahead.</p>");
            //str.AppendLine("Warm Regards,<br />");
            //str.AppendLine("Your Sensei<br />");

            message.Message = MailText;
            var sendEmailResponse = sendEmail(message);
            return sendEmailResponse;
        }
        public static string SendAdminForApproval(SignupInputViewModel details, UserDetail adminDetails)
        {
            EmailHelperInputModel message = new EmailHelperInputModel();
            message.Subject = "Approval Request";
            message.Email = adminDetails.UserName;
            //message.Email = "virtualemployee3145@gmail.com";

            string FilePath = HttpContext.Current.Server.MapPath("~/");
            // FilePath = FilePath.Replace("YourSensei\\", "");
            //FilePath = FilePath + @"Src\YourSensei.Utility\SendMail\EmailTemplate\Notification.html";
            FilePath = FilePath.Substring(0, FilePath.LastIndexOf("\\") + 1);
            FilePath = FilePath + @"bin\EmailTemplate\Notification.html";
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();

            MailText = MailText.Replace("[newusername]","Kobus");
            if (!String.IsNullOrEmpty(details.companyname))
            {
                //MailText = MailText.Replace("[content]", details.FirstName + "is registered himself as company :-" +details.companyname+ "and requested for approval." + "<br>" +
                //    "Please click on this link to see request <a href =http://www.yoursensei.org/#/Main/resetpassword?resetlink" + ">Request Page</a></br>");
                MailText = MailText.Replace("[content]", details.FirstName + "has registered the new company :- " + details.companyname + "and requested for approval." + "<br>" +
                    "Please click on this link to see request <a href =http://www.yoursensei.org/#/Main/resetpassword?resetlink" + ">Request Page</a></br>");  //Content Changed As Given.
            }
            else
            {
                MailText = MailText.Replace("[content]", details.FirstName + "is requested for Approval." + "<br>" +
                    "Please click on this link to see request <a href =http://www.yoursensei.org/#/Main/resetpassword?resetlink" + ">Request Page</a></br>");
            }
            //StringBuilder str = new StringBuilder();
            //str.Append(string.Format("{0},{1}", @"<p><strong>Dear Kobus,</strong>", ""));
            //if (!String.IsNullOrEmpty(details.companyname))
            //{
            //    str.Append(@"<p>" + details.FirstName + "is registered himself as company :-" + details.companyname + "and request for approval </p>");
            //}
            //else
            //{
            //    str.Append(@"<p>" + details.FirstName + "is requested to Approval </p>");
            //}
            //str.Append($"<p>Please click on this link to see request <a href =http://www.yoursensei.org/#/Main/resetpassword?resetlink" + ">Request Page</a> </p></br>");
            //str.AppendLine("Warm Regards,<br />");
            //str.AppendLine("Your Sensei<br />");

            message.Message = MailText;
            var sendEmailResponse = sendEmail(message);
            return sendEmailResponse;
        }

        public static string SendRejectedEmailFromSensei(string email)
        {
            EmailHelperInputModel message = new EmailHelperInputModel();
            message.Subject = "Intimation for rejection";
            //message.Email = "virtualemployee3145@gmail.com";
            message.Email = email;

            string FilePath = HttpContext.Current.Server.MapPath("~/");
            //  FilePath = FilePath.Replace("YourSensei\\", "");
            // FilePath = FilePath + @"Src\YourSensei.Utility\SendMail\EmailTemplate\Notification.html";
            FilePath = FilePath.Substring(0, FilePath.LastIndexOf("\\") + 1);
            FilePath = FilePath + @"bin\EmailTemplate\Notification.html";
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();
            MailText = MailText.Replace("[newusername]", "");
            MailText = MailText.Replace("[content]", string.Format("We regret to inform you that, your request has been rejected!!"));
            //StringBuilder str = new StringBuilder();
            //str.Append(string.Format("We regret to inform you that, your request has been rejected!!"));
            message.Message = MailText;
            var sendEmailResponse = sendEmail(message);
            return sendEmailResponse;
        }

        public static string SendTrainingEventInvitationEmail(TrainingEvent trainingEvent, Employee toEmployee, Employee trainingEventCreator, Employee instructorEmployee)
        {
            try
            {
                EmailHelperInputModel message = new EmailHelperInputModel();
                message.Subject = "Invitation - " + trainingEvent.Name;
                message.Email = toEmployee.Email;
                string FilePath = HttpContext.Current.Server.MapPath("~/");
                //  FilePath = FilePath.Replace("YourSensei\\", "");
                //FilePath = FilePath + @"Src\YourSensei.Utility\SendMail\EmailTemplate\Notification.html";
                FilePath = FilePath.Substring(0, FilePath.LastIndexOf("\\") + 1);
                FilePath = FilePath + @"bin\EmailTemplate\Notification.html";
                StreamReader str = new StreamReader(FilePath);
                string MailText = str.ReadToEnd();
                str.Close();
                MailText = MailText.Replace("[newusername]", toEmployee.FirstName + " " + toEmployee.LastName);
                MailText = MailText.Replace("[content]", trainingEventCreator.FirstName + " " + trainingEventCreator.LastName + " has invited you to join " +
                   trainingEvent.Name + " training event. Below are details of the event.<br><br>" +
                   "Instructor " + instructorEmployee.FirstName + " " + instructorEmployee.LastName + "" + "<br>" +
                   "Scheduled Date " + trainingEvent.startdate.ToString("dd/MM/yyyy") + "<br>" +
                   "Training Notes " + trainingEvent.trainingnotes);

                //StringBuilder str = new StringBuilder();
                //str.AppendLine("<p><strong>Dear " + toEmployee.FirstName + " " + toEmployee.LastName + "</strong>,</p><br />");
                //str.AppendLine("<p>" + trainingEventCreator.FirstName + " " + trainingEventCreator.LastName + " has invited you to join " + " <strong>" +
                //    trainingEvent.Name + "</strong> training event. Below are details of the event.</p>");
                //str.AppendLine("<p><strong>Event Name</strong>: " + trainingEvent.Name + "</p>");
                //str.AppendLine("<p><strong>Instructor</strong>: " + instructorEmployee.FirstName + " " + instructorEmployee.LastName + "</p>");
                //str.AppendLine("<p><strong>Scheduled Date</strong>: " + trainingEvent.startdate.ToString("dd/MM/yyyy") + "</p>");
                //str.AppendLine("<p><strong>Training Notes</strong>: " + trainingEvent.trainingnotes + "</p><br />");

                //str.AppendLine("Warm Regards,<br />");
                //str.AppendLine("Your Sensei<br />");

                message.Message = MailText;
                var sendEmailResponse = sendEmail(message);
                return sendEmailResponse;
            }
            catch(Exception ex)
            {
                return "Failed";
            }
        }

        public static string SendNewEventNotificationEmail(List<Employee> empList, Employee toEmployee, TrainingEvent trainingEvent, Employee trainingEventCreator, Employee instructorEmployee = null)
        {
            EmailHelperInputModel message = new EmailHelperInputModel();
            message.Subject = "Notification - " + trainingEvent.Name;
            message.Email = toEmployee.Email;
            string FilePath = HttpContext.Current.Server.MapPath("~/");
            // FilePath = FilePath.Replace("YourSensei\\", "");
            //FilePath = FilePath + @"Src\YourSensei.Utility\SendMail\EmailTemplate\Notification.html";
            FilePath = FilePath.Substring(0, FilePath.LastIndexOf("\\") + 1);
            FilePath = FilePath + @"bin\EmailTemplate\Notification.html";
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();
            StringBuilder content = new StringBuilder();
            
            content.AppendLine("<p>" + trainingEventCreator.FirstName + " " + trainingEventCreator.LastName + " has created the " + " <strong>" +
                trainingEvent.Name + "</strong> training event for your students. Below are details of the event.</p><br>");
            content.AppendLine("<p><strong>Event Name</strong>: " + trainingEvent.Name + "</p><br>");
            if (instructorEmployee != null)
                content.AppendLine("<p><strong>Instructor</strong>: " + instructorEmployee.FirstName + " " + instructorEmployee.LastName + "</p><br>");
            content.AppendLine("<p><strong>Scheduled Date</strong>: " + trainingEvent.startdate.ToString("dd/MM/yyyy") + "</p><br>");
            content.AppendLine("<p><strong>Training Notes</strong>: " + trainingEvent.trainingnotes + "</p><br>");
            content.AppendLine("<p><strong>Students Invited</strong>: </p><br>");
            content.AppendLine(GetEmpTable(empList).ToString() + "<br>");
            MailText = MailText.Replace("[newusername]", toEmployee.FirstName + " " + toEmployee.LastName);
            MailText = MailText.Replace("[content]", content.ToString());
           

            


            message.Message = MailText;
            var sendEmailResponse = sendEmail(message);
            return sendEmailResponse;
        }

        public static string QuizAssessmentEmailSendToCompanyAdmin(Quiz quizDetail, CompanyDetail compDetails, decimal score, string EmployeeName)
        {
            EmailHelperInputModel message = new EmailHelperInputModel();
            message.Subject = "Quiz - " + quizDetail.Name;
            message.Email = compDetails.email;
            //message.Email = "virtualemployee3145@gmail.com";

            string FilePath = HttpContext.Current.Server.MapPath("~/");
            // FilePath = FilePath.Replace("YourSensei\\", "");
            //FilePath = FilePath + @"Src\YourSensei.Utility\SendMail\EmailTemplate\Notification.html";
            FilePath = FilePath.Substring(0, FilePath.LastIndexOf("\\") + 1);
            FilePath = FilePath + @"bin\EmailTemplate\Notification.html";
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();
            MailText = MailText.Replace("[newusername]", compDetails.companyname);
            MailText = MailText.Replace("[content]", EmployeeName + "has been scored" + score + "credits for the quiz " + quizDetail.Name+".");
            //StringBuilder str = new StringBuilder();
            //str.Append("<p><strong>Dear " + compDetails.companyname + "</strong>,</p><br />");
            //str.Append(@"<p> " + EmployeeName + " has been scored " + score + " credits for the quiz " + quizDetail.Name + "</p>");
            //str.AppendLine("Warm Regards,<br />");
            //str.AppendLine("Your Sensei<br />");

            message.Message = MailText;
            var sendEmailResponse = sendEmail(message);
            return sendEmailResponse;
        }

        public static string QuizAssessmentEmailSendToMentor(Quiz quizDetail, Employee mentor, decimal score, string EmployeeName, Guid empID)
        {
            EmailHelperInputModel message = new EmailHelperInputModel();
            message.Subject = "Quiz - " + quizDetail.Name;
            message.Email = mentor.Email;
            //message.Email = "virtualemployee3145@gmail.com";
            string FilePath = HttpContext.Current.Server.MapPath("~/");
            //  FilePath = FilePath.Replace("YourSensei\\", "");
            // FilePath = FilePath + @"Src\YourSensei.Utility\SendMail\EmailTemplate\Notification.html";
            FilePath = FilePath.Substring(0, FilePath.LastIndexOf("\\") + 1);
            FilePath = FilePath + @"bin\EmailTemplate\Notification.html";
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();
            MailText = MailText.Replace("[newusername]", mentor.FirstName + " " + mentor.LastName);
            MailText = MailText.Replace("[content]", "Good news: Your student" + EmployeeName + " is making progress! This is to notify you that your student has completed " +
                "the" + quizDetail.Name + ". This is a good opportunity for you to congratulate your student and to review any incorrect answers" +
                "he/she may have had – follow the link below to review the test answers:<br><br>" +
                "Test link: <a href=\"" + AngularUrl + "Main/viewquiz?qid=" + quizDetail.ID + "&eid=" + empID + "\">Quiz Assessment" +
                "</a><br><br>" +
                "Next steps: Think about any practical way you can help your student put their new knowledge to the test (with a " +
                "face-to-face discussion about the deeper concepts in this book, an A3 diagram or Kaizen event or suggesting a book that will " +
                "build on their new knowledge).");


            //StringBuilder str = new StringBuilder();
            //str.AppendLine(string.Format("<p><strong>Dear " + mentor.FirstName + " " + mentor.LastName + "</strong>,</p><br />"));
            //str.AppendLine("<p>Good news: Your student " + EmployeeName + " is making progress! This is to notify you that your student completed " +
            //    "the " + quizDetail.Name + ". This is a good opportunity for you to congratulate your student and to review any incorrect answers " +
            //    "he/she may have had – follow the link below to review the test answers:</p>");
            //str.AppendLine("<p>Test link: <a href=\"" + AngularUrl + "Main/viewquiz?qid=" + quizDetail.ID + "&eid=" + empID + "\">Quiz Assessment" +
            //    "</a></p><br />");
            //str.AppendLine("<p>Next steps: Think about any practical way you can help your student put their new knowledge to the test (with a " +
            //    "face-to-face discussion about the deeper concepts in this book, an A3 diagram or Kaizen event or suggesting a book that will " +
            //    "build on their new knowledge).</p><br />");
            //str.AppendLine("Warm Regards,<br />");
            //str.AppendLine("Your Sensei<br />");

            message.Message = MailText;
            var sendEmailResponse = sendEmail(message);
            return sendEmailResponse;
        }

        public static string QuizAssessmentEmailSendToUser(Quiz quizDetail, Employee user, decimal score, string bookTitle)
        {
            EmailHelperInputModel message = new EmailHelperInputModel();
            message.Subject = "Quiz - " + quizDetail.Name;
            message.Email = user.Email;
            //message.Email = "virtualemployee3145@gmail.com";
            string FilePath = HttpContext.Current.Server.MapPath("~/");
            //FilePath = FilePath.Replace("YourSensei\\", "");
            //FilePath = FilePath + @"Src\YourSensei.Utility\SendMail\EmailTemplate\Notification.html";
            FilePath = FilePath.Substring(0, FilePath.LastIndexOf("\\") + 1);
            FilePath = FilePath + @"bin\EmailTemplate\Notification.html";
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();
            MailText = MailText.Replace("[newusername]", user.FirstName + " " + user.LastName);
            MailText = MailText.Replace("[content]", "Congratulations on completing the quiz for" + bookTitle + ". " + "You have earned "
               + score + " Credits which brings you" +
               "closer to your next belt level! Please work with your mentor to plan an A3 or Kaizen event for you to put your new book knowledge to " +
                "the test.");

            //StringBuilder str = new StringBuilder();
            //str.AppendLine(string.Format("<p><strong>Dear " + user.FirstName + " " + user.LastName + "</strong>,</p><br />"));
            //str.AppendLine("<p>Congratulations on completing the quiz for " + bookTitle + ". You have earned " + score + " Credits which brings you " +
            //    "closer to your next belt level! Please work with your mentor to plan an A3 or Kaizen event for you to put your new book knowledge to " +
            //    "the test.</p><br />");
            //str.AppendLine("Warm Regards,<br />");
            //str.AppendLine("Your Sensei");

            message.Message = MailText;
            var sendEmailResponse = sendEmail(message);
            return sendEmailResponse;
        }

        public static string SendWelcomeEmailToEmployee(Employee employee)
        {
            //Fetching Email Body Text from EmailTemplate File.  

            string FilePath = HttpContext.Current.Server.MapPath("~/");
            // FilePath = FilePath.Replace("YourSensei\\", "");
            //FilePath = FilePath + @"Src\YourSensei.Utility\SendMail\EmailTemplate\Welcome.html";
            FilePath = FilePath.Substring(0, FilePath.LastIndexOf("\\") + 1);
            FilePath = FilePath + @"bin\EmailTemplate\Welcome.html";
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();

            //Repalce [newusername] = signup user name   
            MailText = MailText.Replace("[newusername]", employee.FirstName + " " + employee.LastName);
            DateTime expreddate = System.DateTime.Now.AddHours(48.0);
            string datatoencrypt = "email:" + employee.Email + "&expiredate:" + expreddate;
            string encryptdata = Encryption.Encrypt(datatoencrypt, "123456789");

            EmailHelperInputModel message = new EmailHelperInputModel();
            message.Subject = "Welcome to Your Sensei";
            message.Email = employee.Email;
            //StringBuilder str = new StringBuilder();
            //str.AppendLine(string.Format("<p><strong>Dear " + employee.FirstName + " " + employee.LastName + "</strong>,</p><br />"));
            //str.AppendLine("<p>Welcome to Your Sensei and congratulations on taking your first step towards a better stronger you! Please schedule a " +
            //    "discussion with your assigned mentor to review the best way for you to grow at this stage in your career by utilizing the powerful " +
            //    "content of Your Sensei. We are rooting for you!</p><br />");
            //str.AppendLine("<p>Your initial password is <strong>" + ConfigurationManager.AppSettings["DefaultPass"].ToString() + "</strong>. Kindly use " +
            //    "this password to login and change it to your unique one or you can click <strong><a href=\"" + AngularUrl + "Main/resetpassword?" +
            //    "resetlink=" + encryptdata + "\">here</a></strong> to set it.</p><br />");
            //str.AppendLine("Warm Regards,<br />");
            //str.AppendLine("Your Sensei");

            message.Message = MailText;
            var sendEmailResponse = sendEmail(message);
            return sendEmailResponse;
        }

        public static string SendChangePasswordEmail(Employee employee)
        {
            DateTime expreddate = System.DateTime.Now.AddHours(48.0);
            string datatoencrypt = "email:" + employee.Email + "&expiredate:" + expreddate;
            string encryptdata = Encryption.Encrypt(datatoencrypt, "123456789");

            EmailHelperInputModel message = new EmailHelperInputModel();
            message.Subject = "Did you change your password?";
            message.Email = employee.Email;

            string FilePath = HttpContext.Current.Server.MapPath("~/");
            // FilePath = FilePath.Replace("YourSensei\\", "");
            //FilePath = FilePath + @"Src\YourSensei.Utility\SendMail\EmailTemplate\Notification.html";
            FilePath = FilePath.Substring(0, FilePath.LastIndexOf("\\") + 1);
            FilePath = FilePath + @"bin\EmailTemplate\Notification.html";
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();

            MailText = MailText.Replace("[newusername]", employee.FirstName + " " + employee.LastName);
            MailText = MailText.Replace("[content]", "We noticed the password of your account was recently changed. If this was you, you can safely disregard this email," +
                "otherwise please contact to system administrator");

            //StringBuilder str = new StringBuilder();
            //str.AppendLine(string.Format("<p><strong>Dear " + employee.FirstName + " " + employee.LastName + "</strong>,</p><br />"));
            //str.AppendLine("<p>We noticed the password of your account was recently changed. If this was you, you can safely disregard this email, " +
            //    "otherwise please contact to system administrator.</p><br />");
            //str.AppendLine("Warm Regards,<br />");
            //str.AppendLine("Your Sensei");

            message.Message = MailText;
            var sendEmailResponse = sendEmail(message);
            return sendEmailResponse;
        }

        public static string SendTechnivalSupportEmail(TechSupportInputViewModel input)
        {
            EmailHelperInputModel message = new EmailHelperInputModel();
            message.Subject = "Technical Support";
            //message.Email = "kobus@globalturnarounds.com";
            message.Email = input.email;
            string FilePath = HttpContext.Current.Server.MapPath("~/");
            // FilePath = FilePath.Replace("YourSensei\\", "");
            //FilePath = FilePath + @"Src\YourSensei.Utility\SendMail\EmailTemplate\Notification.html";
            FilePath = FilePath.Substring(0, FilePath.LastIndexOf("\\") + 1);
            FilePath = FilePath + @"bin\EmailTemplate\Notification.html";
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();

            MailText = MailText.Replace("[newusername]", "Kobus");
            MailText = MailText.Replace("[content]", input.firstName + " " + input.lastName + " has Willing to contact us with Details Below" +
                "Email" + input.email);
            if (input.phone != null)
            {
                MailText = MailText.Replace("[content]", "Phone" + input.phone +
                    "How can we help you?" + input.helpbox);

            }
            if (input.additional != null)
            {
                MailText = MailText.Replace("[content]", "Additional Detail" + input.additional);
            }
            //StringBuilder str = new StringBuilder();
            //str.AppendLine("<p><strong>Dear Kobus");
            //str.AppendLine("<p>" + input.firstName + " " + input.lastName + " has Willing to contact us with Details Below");
            //str.AppendLine("<p><strong>Email</strong>: " + input.email + "</p>");
            //if (input.phone != null)
            //{
            //    str.AppendLine("<p><strong>Phone</strong>: " + input.phone + "</p>");
            //}

            //str.AppendLine("<p><strong>How can we help you?</strong>: " + input.helpbox + "</p>");
            //if (input.additional != null)
            //{
            //    str.AppendLine("<p><strong>Additional Detail</strong>: " + input.additional + "</p><br />");
            //}


            //str.AppendLine("Warm Regards,<br />");
            //str.AppendLine("Your Sensei<br />");

            message.Message = MailText;
            var sendEmailResponse = sendEmail(message);
            return sendEmailResponse;
        }

        public static string SendEmailforDollarApprove(Employee toEmployee, TrainingEvent trainingEvent)
        {
            EmailHelperInputModel message = new EmailHelperInputModel();
            message.Subject = "Event Submitted - " + trainingEvent.Name;
            message.Email = toEmployee.Email;
            //message.Email = "tarunarora@virtualemployee.com";

            string FilePath = HttpContext.Current.Server.MapPath("~/");
            // FilePath = FilePath.Replace("YourSensei\\", "");
            //FilePath = FilePath + @"Src\YourSensei.Utility\SendMail\EmailTemplate\Notification.html";
            FilePath = FilePath.Substring(0, FilePath.LastIndexOf("\\") + 1);
            FilePath = FilePath + @"bin\EmailTemplate\Notification.html";
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();
            MailText = MailText.Replace("[newusername]", toEmployee.FirstName + " " + toEmployee.LastName);
            MailText = MailText.Replace("[content]", "The " + trainingEvent.Name + " training event has been Approved.<br>" +
                "Please click <strong><a href=\"" + AngularUrl + "/Main/studentevent?id=" + trainingEvent.ID + "\">here</a></strong> to review and approved this Event.");
            //StringBuilder str = new StringBuilder();
            //str.AppendLine("<p><strong>Dear " + toEmployee.FirstName + " " + toEmployee.LastName + "</strong>,</p><br />");
            //str.AppendLine("<p>The <strong>" + trainingEvent.Name + "</strong> training event has been Approved"  + "</p><br />");

            //str.AppendLine("Please click <strong><a href=\"" + AngularUrl + "/Main/studentevent?id=" + trainingEvent.ID + "\">here</a></strong> to review and approved the A3 Event.<br />");



            //str.AppendLine("Warm Regards,<br />");
            //str.AppendLine("Your Sensei<br />");

            message.Message = MailText;
            var sendEmailResponse = sendEmail(message);
            return sendEmailResponse;
        }

        public static string SendA3FormUpdateNotifyEmail(Employee userAccount, TrainingEventA3Diagram a3event, string mentorEmail, string mentorName)
        {
            var subject = "User Added/Updated A3 Form";
            string FilePath = HttpContext.Current.Server.MapPath("~/");
            // FilePath = FilePath.Replace("YourSensei\\", "");
            //FilePath = FilePath + @"Src\YourSensei.Utility\SendMail\EmailTemplate\Notification.html";
            FilePath = FilePath.Substring(0, FilePath.LastIndexOf("\\") + 1);
            FilePath = FilePath + @"bin\EmailTemplate\Notification.html";
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();
            MailText = MailText.Replace("[newusername]", mentorName);
            MailText = MailText.Replace("[content]", String.Format("<b>User with email '{0}' has added/updated an A3 Form.</b><br />", userAccount.Email) + "<br><br>" +
                "First Name: " + (userAccount.FirstName ?? "") + "<br >" +
                "Last Name: " + (userAccount.LastName ?? "") + "<br >" +
                "Email Address: " + (userAccount.Email ?? "") + "<br><br>" +
                "Please click <strong><a href=\"" + AngularUrl + "/Main/studentevent?id=" + a3event.TrainingEventID + "\">here</a></strong> to review the A3 Event.<br><br>" +
                "The added/updated Form for this User is shown below.<br >" +
                "Background: " + (a3event.Background ?? "") + "<br >" +
                "Current Condition: " + (a3event.CurrentCondition ?? "") + "<br>" +
                "Analyses: " + (a3event.Analyses ?? "") + "<br >" +
                "FollowUp: " + (a3event.FollowUp ?? "") + "<br >" +
                "Goal: " + (a3event.Goal ?? "") + "<br>" +
                "Plan: " + (a3event.Plan ?? "") + "<br >" +
                "Proposal: " + (a3event.Proposal ?? "") + "<br >" +
                "Dollar Impacted: " + (a3event.DollarImpacted) + "<br>");
            //StringBuilder str = new StringBuilder();
            //str.Append(String.Format("<b>User with email '{0}' has added/updated an A3 Form.</b><br />", userAccount.Email));
            //str.Append("<br />");

            //str.Append("First Name: " + (userAccount.FirstName ?? "") + "<br />");
            //str.Append("Last Name: " + (userAccount.LastName ?? "") + "<br />");

            //str.Append("Email Address: " + (userAccount.Email ?? "") + "<br />");
            //str.Append("<br />");
            //str.AppendLine("Please click <strong><a href=\"" + AngularUrl + "/Main/studentevent?id=" + a3event.TrainingEventID + "\">here</a></strong> to review the A3 Event.<br />");
            //str.Append("<br />");
            //str.Append("The added/updated Form for this User is shown below.<br />");
            //str.Append("<br />");
            //str.Append("Background: " + (a3event.Background ?? "") + "<br />");
            //str.Append("Current Condition: " + (a3event.CurrentCondition ?? "") + "<br />");
            //str.Append("Analyses: " + (a3event.Analyses ?? "") + "<br />");
            //str.Append("FollowUp: " + (a3event.FollowUp ?? "") + "<br />");
            //str.Append("Goal: " + (a3event.Goal ?? "") + "<br />");
            //str.Append("Plan: " + (a3event.Plan ?? "") + "<br />");
            //str.Append("Proposal: " + (a3event.Proposal ?? "") + "<br />");
            //str.Append("Dollar Impacted: " + (a3event.DollarImpacted) + "<br />");
            ////str.Append("AssignedTo: " + (a3event.Analyses ?? "") + "<br />");
            //str.Append("<br />");
            //str.AppendLine("Warm Regards,<br />");
            //str.AppendLine("Your Sensei<br />");
            EmailHelperInputModel message = new EmailHelperInputModel();
            message.Email = mentorEmail;
            message.Message = MailText;
            message.Subject = subject;
            var sendEmailResponse = sendEmail(message);
            return sendEmailResponse;
        }

        public static string SendEmailforA3Communication(string email, string eventid, string subject,  string receiverfullname, string senderfullname, string usermessage, bool ismentor)
        {

            EmailHelperInputModel message = new EmailHelperInputModel();
            message.Subject = subject;
            message.Email = email;
            //message.Email = "tarunarora@virtualemployee.com";

            string FilePath = HttpContext.Current.Server.MapPath("~/");
            // FilePath = FilePath.Replace("YourSensei\\", "");
            //FilePath = FilePath + @"Src\YourSensei.Utility\SendMail\EmailTemplate\Notification.html";
            FilePath = FilePath.Substring(0, FilePath.LastIndexOf("\\") + 1);
            FilePath = FilePath + @"bin\EmailTemplate\Notification.html";
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();
            MailText = MailText.Replace("[newusername]", receiverfullname);
            if (ismentor)
                MailText = MailText.Replace("[content]", "<strong>" + senderfullname + " has sent you a message.</strong>: " + usermessage + "<br>Please click <strong><a href=\"" + AngularUrl + "/Main/create-event?id=" + eventid + "\">here</a></strong> to reply this.<br>");
            else
                MailText = MailText.Replace("[content]", "<strong>" + senderfullname + " has sent you a message.</strong>: " + usermessage + "<br>Please click <strong><a href=\"" + AngularUrl + "/Main/studentevent?id=" + eventid + "\">here</a></strong> to reply this.<br>");
            //StringBuilder str = new StringBuilder();
            //str.AppendLine(string.Format("<p><strong>Dear " + receiverfullname + "</strong>,</p><br />"));

            //str.AppendLine();
            //if(ismentor)
            //    str.AppendLine("Please click <strong><a href=\"" + AngularUrl + "/Main/create-event?id=" + eventid + "\">here</a></strong> to reply this.<br />");
            //else
            //    str.AppendLine("Please click <strong><a href=\"" + AngularUrl + "/Main/studentevent?id=" + eventid + "\">here</a></strong> to reply this.<br />");





            // str.AppendLine("Warm Regards,<br />");
            //str.AppendLine("Your Sensei<br />");

            message.Message = MailText;
            var sendEmailResponse = sendEmail(message);
            return sendEmailResponse;
        }
        public static string SendEmailToPlanSubscriber(SubscriberEmailInputModel subscriberEmailInputModel)
        {
            EmailHelperInputModel message = new EmailHelperInputModel();
            message.Subject = "Plan Subscription Confirmation";
            message.Email = subscriberEmailInputModel.SubscriberEmail;
            //message.Email = "tarunarora@virtualemployee.com";
            string FilePath = HttpContext.Current.Server.MapPath("~/");
            // FilePath = FilePath.Replace("YourSensei\\", "");
            // FilePath = FilePath + @"Src\YourSensei.Utility\SendMail\EmailTemplate\Notification.html";
            FilePath = FilePath.Substring(0, FilePath.LastIndexOf("\\") + 1);
            FilePath = FilePath + @"bin\EmailTemplate\Notification.html";
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();
            MailText = MailText.Replace("[newusername]", subscriberEmailInputModel.SubscriberName);
            MailText = MailText.Replace("[content]", "<strong> Thanks for subscription. Here is your confirmation for <h6> " + subscriberEmailInputModel.PlanName + " plan.</h6></strong><br><br>" +
                "<strong>Plan Name:</strong> " + subscriberEmailInputModel.PlanName + "<br >" +
                "<strong>Activation Date:</strong> " + subscriberEmailInputModel.ActivationDate.Date + "<br >" +
                "<strong>Expiry Date:</strong> " + subscriberEmailInputModel.ExpiryDate.Date + "<br><br>" +
                "<strong>Expired In:</strong> " + subscriberEmailInputModel.TotalNumberOfPlanDays + "<br >" +
                "<strong>Number of Employees:</strong> " + subscriberEmailInputModel.TotalNumberOfEmployees + "<br>" +
                "<strong>Number of Mentors:</strong> " + subscriberEmailInputModel.TotalNumberOfMentor + "<br >" +
                "<strong>Accessible Features:</strong> " + subscriberEmailInputModel.AccessableFeatures + "<br >");

            //StringBuilder str = new StringBuilder();
            //str.AppendLine(string.Format("<p><strong>Dear " + subscriberEmailInputModel.SubscriberName + "</strong>,</p><br />"));

            //str.AppendLine("<p><strong>Thanks for subscription. Here is your confirmation for <h6>" + subscriberEmailInputModel.PlanName + " plan.</h6></strong></p><br />");

            //str.AppendLine("<p><strong>Plan Name:</strong> " + subscriberEmailInputModel.PlanName + "</p><br />");
            //str.AppendLine("<p><strong>Activation Date:</strong> " + subscriberEmailInputModel.ActivationDate.Date + "</p><br />");
            //str.AppendLine("<p><strong>Expiry Date:</strong> " + subscriberEmailInputModel.ExpiryDate.Date + "</p><br />");
            //str.AppendLine("<p><strong>Expired In:</strong> " + subscriberEmailInputModel.TotalNumberOfPlanDays + "</p> days<br />");
            //str.AppendLine("<p><strong>Number of Employees:</strong> " + subscriberEmailInputModel.TotalNumberOfEmployees + "</p> Employees.<br />");
            //str.AppendLine("<p><strong>Number of Mentors:</strong> " + subscriberEmailInputModel.TotalNumberOfMentor + "</p> Mentors.<br />");

            //str.AppendLine("<p><strong>Accessible Features:</strong> " + subscriberEmailInputModel.AccessableFeatures + "</p><br />");



            //str.AppendLine("Warm Regards,<br />");
            //str.AppendLine("Your Sensei<br />");

            message.Message = MailText;
            var sendEmailResponse = sendEmail(message);
            return sendEmailResponse;
        }
        public static string SendEmailToRenewPlan(SubscriberEmailInputModel subscriberEmailInputModel)
        {
            EmailHelperInputModel message = new EmailHelperInputModel();
            message.Subject = "Plan Subscription Confirmation";
            message.Email = subscriberEmailInputModel.SubscriberEmail;
            //message.Email = "tarunarora@virtualemployee.com";
            string FilePath = HttpContext.Current.Server.MapPath("~/");
            //  FilePath = FilePath.Replace("YourSensei\\", "");
            //  FilePath = FilePath + @"Src\YourSensei.Utility\SendMail\EmailTemplate\Notification.html";
            FilePath = FilePath.Substring(0, FilePath.LastIndexOf("\\") + 1);
            FilePath = FilePath + @"bin\EmailTemplate\Notification.html";
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();
            MailText = MailText.Replace("[newusername]", subscriberEmailInputModel.SubscriberName);
            MailText = MailText.Replace("[content]", "<strong>Thanks for renew the subscription. This renewal will be effective after the expiry of your old subscription." +
                " Here is your confirmation for <h6>" + subscriberEmailInputModel.PlanName + " plan.</h6></ strong ><br><br>" +
                "<strong>Plan Name:</strong> " + subscriberEmailInputModel.PlanName + "<br >" +
                "<strong>Activation Date:</strong> " + subscriberEmailInputModel.ActivationDate.Date + "<br >" +
                "<strong>Expiry Date:</strong> " + subscriberEmailInputModel.ExpiryDate.Date + "<br><br>" +
                "<strong>Expired In:</strong> " + subscriberEmailInputModel.TotalNumberOfPlanDays + "<br >" +
                "<strong>Number of Employees:</strong> " + subscriberEmailInputModel.TotalNumberOfEmployees + "<br>" +
                "<strong>Number of Mentors:</strong> " + subscriberEmailInputModel.TotalNumberOfMentor + "<br >" +
                "<strong>Accessible Features:</strong> " + subscriberEmailInputModel.AccessableFeatures + "<br >");
            //StringBuilder str = new StringBuilder();
            //str.AppendLine(string.Format("<p><strong>Dear " + subscriberEmailInputModel.SubscriberName + "</strong>,</p><br />"));

            //str.AppendLine("<p><strong>Thanks for renew the subscription. This renewal will be effective after the expiry of your old subscription." +
            //   " Here is your confirmation for <h6>" + subscriberEmailInputModel.PlanName + " plan.</h6></strong></p><br />");

            //str.AppendLine("<p><strong>Plan Name:</strong> " + subscriberEmailInputModel.PlanName + "</p><br />");
            //str.AppendLine("<p><strong>Activation Date:</strong> " + subscriberEmailInputModel.ActivationDate.Date + "</p><br />");
            //str.AppendLine("<p><strong>Expiry Date:</strong> " + subscriberEmailInputModel.ExpiryDate.Date + "</p><br />");
            //str.AppendLine("<p><strong>Expired In:</strong> " + subscriberEmailInputModel.TotalNumberOfPlanDays + "</p> days<br />");
            //str.AppendLine("<p><strong>Number of Employees:</strong> " + subscriberEmailInputModel.TotalNumberOfEmployees + "</p> Employees.<br />");
            //str.AppendLine("<p><strong>Number of Mentors:</strong> " + subscriberEmailInputModel.TotalNumberOfMentor + "</p> Mentors.<br />");

            //str.AppendLine("<p><strong>Accessible Features:</strong> " + subscriberEmailInputModel.AccessableFeatures + "</p><br />");



            //str.AppendLine("Warm Regards,<br />");
            //str.AppendLine("Your Sensei<br />");

            message.Message = MailText;
            var sendEmailResponse = sendEmail(message);
            return sendEmailResponse;
        }

        public static string SendWeeklyUpdateToUsers(GetDashboardBeltRuleViewModel Pyramid, List<CreditStandingViewModel> credittable, EmployeeResponseViewModel emp)
        {
            EmailHelperInputModel message = new EmailHelperInputModel();
            message.Subject = "Plan Subscription Confirmation";
            message.Email = emp.Email;
            //message.Email = "tarunarora@virtualemployee.com";
            string FilePath = HttpContext.Current.Server.MapPath("~/");
            // FilePath = FilePath.Replace("YourSensei\\", "");
            // FilePath = FilePath + @"Src\YourSensei.Utility\SendMail\EmailTemplate\WeeklyUpdateEmail.html";
            FilePath = FilePath.Substring(0, FilePath.LastIndexOf("\\") + 1);
            FilePath = FilePath + @"bin\EmailTemplate\Notification.html";
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();
            StringBuilder table = new StringBuilder();
            foreach (var item in credittable)
            {
              table.Append("<tr style='font - size:12px; font - family: 'Lato', Helvetica, Arial, sans - serif;'><td align = 'left' valign = 'middle' style = 'padding: 5px;' > Seamus Daley </td><td align = 'left' valign = 'middle' style = 'padding: 5px;' > Kobus ven der zel </td >< td align = 'left' valign = 'middle' style = 'padding: 5px;' > Belt </td></tr>");
            }










            MailText = MailText.Replace("[newusername]", emp.FirstName + "" + emp.LastName);
            MailText = MailText.Replace("[currentdate]", string.Format("{0:MMMM dd, yyyy}", DateTime.Now));
            MailText = MailText.Replace("[totalcredit]", Pyramid.totalcredit.ToString());
            MailText = MailText.Replace("[creditwant]", Pyramid.creditwant.ToString());
            MailText = MailText.Replace("[CreditTables]", table.ToString());
                

            message.Message = MailText;
            var sendEmailResponse = sendEmail(message);
            return sendEmailResponse;
        }

        //Kaizen notification Email (by Saurabh)--------------------------------------------------------

        public static string SendKaizenFormUpdateNotifyEmail(Employee userAccount, TrainingEventKaizenDiagram kaizenevent, string mentorEmail, string mentorName)
        {
            var subject = "User Added/Updated Kaizen Form";
            string FilePath = HttpContext.Current.Server.MapPath("~/");
            FilePath = FilePath.Substring(0, FilePath.LastIndexOf("\\") + 1);
            FilePath = FilePath + @"bin\EmailTemplate\Notification.html";
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();
            MailText = MailText.Replace("[newusername]", mentorName);
            MailText = MailText.Replace("[content]", String.Format("<b>User with email '{0}' has added/updated an A3 Form.</b><br />", userAccount.Email) + "<br><br>" +
                "First Name: " + (userAccount.FirstName ?? "") + "<br >" +
                "Last Name: " + (userAccount.LastName ?? "") + "<br >" +
                "Email Address: " + (userAccount.Email ?? "") + "<br><br>" +
                "Please click <strong><a href=\"" + AngularUrl + "/Main/studentevent?id=" + kaizenevent.TrainingEventID + "\">here</a></strong> to review the A3 Event.<br><br>" +
                "The added/updated Form for this User is shown below.<br >" +
                "Define The Problem: " + (kaizenevent.DefineTheProblem ?? "") + "<br >" +
                "Current Condition: " + (kaizenevent.CurrentCondition ?? "") + "<br>" +
                "Analysis: " + (kaizenevent.Analysis ?? "") + "<br >" +
                "FollowUp: " + (kaizenevent.FollowUp ?? "") + "<br >" +
                "Goal: " + (kaizenevent.Goal ?? "") + "<br>" +
                "Implementation Plan: " + (kaizenevent.ImplementationPlan ?? "") + "<br >" +
                "Action Item Timeline: " + (kaizenevent.ActionItemTimeline ?? "") + "<br >" +
                "Dollar Impacted: " + (kaizenevent.DollarImpacted) + "<br>");
            
            EmailHelperInputModel message = new EmailHelperInputModel();
            message.Email = mentorEmail;
            message.Message = MailText;
            message.Subject = subject;
            var sendEmailResponse = sendEmail(message);
            return sendEmailResponse;
        }

        public static string SendEmailforKaizenCommunication(string email, string eventid, string receiverfullname, string senderfullname, string usermessage, bool ismentor)
        {
            EmailHelperInputModel message = new EmailHelperInputModel();
            message.Subject = "Kaizen Communication Notification";
            message.Email = email;
            //message.Email = "tarunarora@virtualemployee.com";

            string FilePath = HttpContext.Current.Server.MapPath("~/");
            FilePath = FilePath.Substring(0, FilePath.LastIndexOf("\\") + 1);
            FilePath = FilePath + @"bin\EmailTemplate\Notification.html";
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();
            MailText = MailText.Replace("[newusername]", receiverfullname);
            if (ismentor)
                MailText = MailText.Replace("[content]", "<strong>" + senderfullname + " has sent you a message.</strong>: " + usermessage + "<br>Please click <strong><a href=\"" + AngularUrl + "/Main/create-event?id=" + eventid + "\">here</a></strong> to reply this.<br>");
            else
                MailText = MailText.Replace("[content]", "<strong>" + senderfullname + " has sent you a message.</strong>: " + usermessage + "<br>Please click <strong><a href=\"" + AngularUrl + "/Main/studentevent?id=" + eventid + "\">here</a></strong> to reply this.<br>");
            message.Message = MailText;
            var sendEmailResponse = sendEmail(message);
            return sendEmailResponse;
        }
    }
}
