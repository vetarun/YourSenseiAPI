using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using YourSensei.Data;
using YourSensei.Utility;
using YourSensei.ViewModel;
using YourSensei.ViewModel.Dashboard;

namespace YourSensei.WorkQueueProcessor
{
    public class WeeklyEmailUpdate : IWeeklyEmailUpdate
    {
        private readonly YourSensei_DBEntities _context;
        public WeeklyEmailUpdate(YourSensei_DBEntities context)
        {
            _context = context;
        }

        public List<EmployeeResponseViewModel> GetAllEmployee()
        {
            try
            {
                var data = (from e in _context.Employees
                            join ud in _context.UserDetails on e.ID equals ud.EmployeeID
                            join r in _context.UserCategories on ud.UserType equals r.Id
                            select new EmployeeResponseViewModel()
                            {
                                Id = e.ID.ToString(),
                                FirstName = e.FirstName,
                                LastName = e.LastName,
                                RoleId = r.Description.ToString(),
                                MentorId = e.MentorId.ToString(),
                                IsMentor = e.IsMentor,
                                EmployeeCode = e.EmployeeCode,
                                Credit = e.CreditScore,
                                IsActive = e.IsActive,
                                Email = e.Email,
                                CompanyId = e.CompanyId.ToString(),
                                UserCategory = ud.UserType.ToString(),
                                OtherRole = e.OtherRole,
                                MemberID = e.MemberID,
                                userDetialID = ud.ID.ToString()
                            }).ToList();

                return data;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public string SendWeeklyEmail(GetDashboardBeltRuleViewModel Pyramid, List<CreditStandingViewModel> credittable, EmployeeResponseViewModel emp)
        {
            try
            {
                EmailHelperInputModel message = new EmailHelperInputModel();
                message.Subject = "Weekly Update";
                message.Email = emp.Email;
                //message.Email = "abhisheksharma1@virtualemployee.com";
                //string FilePath = HttpContext.Current.Server.MapPath("~/");
                string FilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                //FilePath = FilePath.Replace("YourSensei.Actioner\\bin\\Debug\\YourSensei.WorkQueueProcessor.dll", "");
                FilePath = FilePath.Substring(0, FilePath.LastIndexOf("\\") + 1);
                FilePath = FilePath + @"EmailTemplate\WeeklyUpdateEmail.html";
                //FilePath = FilePath + @"YourSensei.Utility\SendMail\EmailTemplate\WeeklyUpdateEmail.html";
                //string FilePath = "D:\\Saurabh\\YourSensei Project\\YourSenseiWebAPI\\Src\\YourSensei.Utility\\SendMail\\EmailTemplate\\WeeklyUpdateEmail.html";
                StreamReader str = new StreamReader(FilePath);
                string MailText = str.ReadToEnd();
                str.Close();
                StringBuilder beltList = new StringBuilder();
                int youarehereindex = 0;
                int beltwidth = 60;
                foreach (var belt in Pyramid.ListOfBelt)
                {
                    beltList.Append("<tr style='font - size:12px; font - family: 'Lato', Helvetica, Arial, sans - serif;'>");
                    if (youarehereindex == Pyramid.indexofyouarehere)
                        beltList.Append("<td align = 'right' valign = 'middle'><img src = 'http://api.yoursensei.org/Images/yah.jpg' style = 'vertical-align: bottom;'></td>");
                    else
                        beltList.Append("<td align='center' valign='middle' style='padding: 5px;'></td>");


                    beltList.Append("<td align = 'center' valign = 'middle' ><img  src = 'http://api.yoursensei.org/PyramidImages/" + belt.BeltName.Split(' ').FirstOrDefault() + ".jpg'></td>" +
                        "<td align = 'left' valign = 'middle' style = 'padding: 5px;' >" + belt.TotalCredit + " Credits + More </td></tr >");
                    youarehereindex++;
                    if (youarehereindex == 1)
                        beltwidth = beltwidth + 30;
                    else
                        beltwidth = beltwidth + 20;

                }
                StringBuilder BeltProgress = new StringBuilder();
                string currentbelt = string.Empty;
                string nextbelt = string.Empty;
                if (Pyramid.indexofyouarehere != -1)
                {
                    currentbelt = Pyramid.ListOfBelt[Pyramid.indexofyouarehere].BeltName;
                    if (Pyramid.indexofyouarehere != 0)
                        nextbelt = Pyramid.ListOfBelt[Pyramid.indexofyouarehere - 1].BeltName;
                    BeltProgress.Append("only ");
                    if (Pyramid.creditwant > 0)
                        BeltProgress.Append(Pyramid.creditwant + " credit");
                    if (Pyramid.creditwant > 0 && Pyramid.a3want > 0)
                        BeltProgress.Append(" and ");
                    if (Pyramid.a3want > 0)
                        BeltProgress.Append(Pyramid.a3want + " A3");
                    if ((Pyramid.a3want > 0 && Pyramid.kaizenwant > 0) || (Pyramid.creditwant > 0 && Pyramid.kaizenwant > 0))
                        BeltProgress.Append(" and ");
                    if (Pyramid.kaizenwant > 0)
                        BeltProgress.Append(Pyramid.kaizenwant + " Kaizen required for your " + Pyramid.ListOfBelt[Pyramid.indexofyouarehere].BeltName + " - good luck!");
                }


                decimal? CILastWeek = 0;
                StringBuilder table = new StringBuilder();
                foreach (var item in credittable)
                {
                    if (item.EmployeeID == new Guid(emp.Id))
                        CILastWeek = item.CILastWeek;
                    table.Append("<tr style='font - size:12px; font - family: 'Lato', Helvetica, Arial, sans - serif;'>" +
                        "<td align = 'left' valign = 'middle' style = 'padding: 5px;' >" + item.EmployeeName + "</td>" +
                        "<td align = 'left' valign = 'middle' style = 'padding: 5px;' >" + item.MentorName + "</td >");
                    if(item.BeltName!= "")
                        table.Append("<td align = 'left' valign = 'middle' style = 'padding: 5px; width: 40px; height: 40px;' ><img style = 'width: 40px;' src = 'http://api.yoursensei.org/BeltImages/" + item.BeltName.Split(' ').FirstOrDefault()+".png' style = 'vertical-align: bottom;'></td>");
                    else
                        table.Append("<td align = 'left' valign = 'middle' style = 'padding: 5px; width: 40px; height: 40px;' ></td>");


                    table.Append("<td align = 'left' valign = 'middle' style = 'padding: 5px;' >" + item.CICredits + "</td>" +
                       "<td align = 'left' valign = 'middle' style = 'padding: 5px;' >" + item.CILastWeek + "</td>" +
                       "<td align = 'left' valign = 'middle' style = 'padding: 5px;' >" + item.DollarImpacted + "</td></tr>");

                }










                MailText = MailText.Replace("[newusername]", emp.FirstName + " " + emp.LastName);
                MailText = MailText.Replace("[currentdate]", string.Format("{0:MMMM dd, yyyy}", DateTime.Now));
                MailText = MailText.Replace("[totalcredit]", Pyramid.totalcredit.ToString());
                MailText = MailText.Replace("[currentbelt]", currentbelt);
                MailText = MailText.Replace("[nextbelt]", nextbelt);
                MailText = MailText.Replace("[beltlist]", beltList.ToString());
                MailText = MailText.Replace("[beltprogress]", BeltProgress.ToString());
                MailText = MailText.Replace("[creditwant]", Pyramid.creditwant.ToString());
                MailText = MailText.Replace("[CreditTables]", table.ToString());
                MailText = MailText.Replace("[CILastWeek]", CILastWeek.ToString());

                message.Message = MailText.ToString();


                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(ConfigurationManager.AppSettings["FromMailAddress"].ToString());
                mail.To.Add(message.Email);

                //todo dynamic subject
                mail.Subject = message.Subject;
                string Body = message.Message;
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
                return "email sent!";

            }
            catch (Exception ex)
            {

                return ex.Message;
            }

        }
    }
}
