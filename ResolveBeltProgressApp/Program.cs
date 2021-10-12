
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResolveBeltProgressApp
{
    class Program
    {
       
        static void Main(string[] args)
        {

            YourSensei_DBEntities1 _context = new YourSensei_DBEntities1();
            try
            {
                var allCompanyUsers = (from ud in _context.UserDetails
                                       join e in _context.Employees on ud.EmployeeID equals e.ID
                                       where ud.IsActive == true && e.IsActive == true
                                       select new { e.CompanyId, userDetailID = ud.ID, employeeID = e.ID }).ToList();

                foreach (var item in allCompanyUsers)
                {
                    List<BeltRule> beltRules = new List<BeltRule>();
                    if (item.CompanyId == Guid.Empty)
                        beltRules = (from br in _context.BeltRules
                                     where br.UserDetailID == item.userDetailID && br.IsActive == true
                                     select br).OrderBy(a => a.OrderValue).ToList();
                    else
                        beltRules = (from br in _context.BeltRules
                                     where br.CompanyID == item.CompanyId && br.IsActive == true
                                     select br).OrderBy(a => a.OrderValue).ToList();

                    foreach (var belt in beltRules)
                    {
                        DateTime creditAwardedDate = DateTime.MinValue;
                        DateTime a3ClosedDate = DateTime.MinValue;
                        DateTime kaizenClosedDate = DateTime.MinValue;

                        List<CreditLog> creditLogs = (from cl in _context.CreditLogs
                                                      where cl.UserDetailID == item.userDetailID
                                                      select cl).OrderBy(a => a.AwardedDate).ToList();
                        decimal sumCredit = 0;
                        foreach (var cl in creditLogs)
                        {
                            sumCredit += cl.Credit;
                            if (sumCredit >= belt.TotalCredit)
                            {
                                creditAwardedDate = cl.AwardedDate;
                                break;
                            }
                        }

                        List<TrainingEvent> a3TrainingEvents = (from tea in _context.TrainingEventAttendees
                                                                join te in _context.TrainingEvents on tea.TrainigEventID equals te.ID
                                                                where te.TrainingEventFormatID == new Guid("6F9F04CC-198E-479C-A93F-6C3C0A359194")
                                                                  && te.Isclosed == true && te.IsActive == true && tea.EmployeeID == item.employeeID
                                                                select te).OrderBy(a => a.ClosedDate).Take(belt.TotalA3).ToList();
                        if (a3TrainingEvents != null && a3TrainingEvents.Count > 0)
                            a3ClosedDate = Convert.ToDateTime(a3TrainingEvents[a3TrainingEvents.Count - 1].ClosedDate);

                        List<TrainingEvent> kaizenTrainingEvents = (from tea in _context.TrainingEventAttendees
                                                                    join te in _context.TrainingEvents on tea.TrainigEventID equals te.ID
                                                                    where te.TrainingEventFormatID == new Guid("5518993A-EFC0-4AD0-BCD7-BEAEA42CC2CE")
                                                                      && te.Isclosed == true && te.IsActive == true && tea.EmployeeID == item.employeeID
                                                                    select te).OrderBy(a => a.ClosedDate).Take(belt.TotalKaizen).ToList();
                        if (kaizenTrainingEvents != null && kaizenTrainingEvents.Count > 0)
                            kaizenClosedDate = Convert.ToDateTime(kaizenTrainingEvents[kaizenTrainingEvents.Count - 1].ClosedDate);

                        if (creditAwardedDate != DateTime.MinValue && a3ClosedDate != DateTime.MinValue && kaizenClosedDate != DateTime.MinValue)
                        {
                            bool hasRecord = (from bal in _context.BeltAchievementLogs
                                              where bal.BeltRuleID == belt.ID && bal.UserDetailID == item.userDetailID
                                              select bal).Any();
                            if (!hasRecord)
                            {
                                DateTime latestDate = new[] { creditAwardedDate, a3ClosedDate, kaizenClosedDate }.Max();
                                _context.BeltAchievementLogs.Add(new BeltAchievementLog
                                {
                                    CompanyID = item.CompanyId,
                                    AchievedDate = latestDate,
                                    BeltRuleID = belt.ID,
                                    UserDetailID = item.userDetailID
                                });
                                _context.SaveChanges();
                            }
                        }
                    }
                }

                //var allCompanyUsers = (from ud in _context.UserDetails
                //                       join e in _context.Employees on ud.EmployeeID equals e.ID
                //                       where (ud.UserType == new Guid("4BA19173-94CD-4222-AF7C-60C91D446F8E")
                //                       || ud.UserType == new Guid("5C37CF64-F617-4399-BB68-645B0C3969A2")
                //                       || ud.UserType == new Guid("99F9AEB1-9BE6-4E82-8671-CA3DF4DF16CB") && ud.IsActive == true && e.IsActive == true)
                //                       select new { e.CompanyId, userDetailID = ud.ID, employeeID = e.ID }
                //                             ).ToList();

                //if (allCompanyUsers != null)
                //{
                //    foreach (var items in allCompanyUsers)
                //    {
                //        TotalA3AndKaizenViewModel a3kaizencred = getTotalCreditA3AndKaizen(Convert.ToString(items.employeeID), Convert.ToString(items.userDetailID));
                //        int beltID = getBeltID(a3kaizencred.TotalCredits, a3kaizencred.TotalA3, a3kaizencred.TotalKaizen,
                //            Convert.ToString(items.CompanyId), Convert.ToString(items.userDetailID));
                //        bool isAlraedyAchieved = _context.BeltAchievementLogs.Any(d => d.UserDetailID == items.userDetailID && d.BeltRuleID == beltID);
                //        if (beltID != -1 && isAlraedyAchieved == false)
                //        {
                //            _context.BeltAchievementLogs.Add(new BeltAchievementLog
                //            {
                //                CompanyID = items.CompanyId,
                //                AchievedDate = DateTime.UtcNow,
                //                BeltRuleID = beltID,
                //                UserDetailID = items.userDetailID
                //            });
                //            _context.SaveChanges();
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }

            // Go to http://aka.ms/dotnet-get-started-console to continue learning how to build a console app! 
        }
    }
}
