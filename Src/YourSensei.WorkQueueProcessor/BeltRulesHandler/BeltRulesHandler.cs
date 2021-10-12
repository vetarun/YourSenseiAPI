using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using YourSensei.Data;
using YourSensei.Utility;
using YourSensei.ViewModel.Dashboard;

namespace YourSensei.WorkQueueProcessor.BeltAchievementHandler
{
    public class BeltRulesHandler : IBeltRulesHandler
    {
        private readonly YourSensei_DBEntities _context;
        public BeltRulesHandler(YourSensei_DBEntities context)
        {
            _context = context;
        }


        public void ProcessBeltAchievement()
        {
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
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public TotalA3AndKaizenViewModel getTotalCreditA3AndKaizen(string EmployeeID, string userdetailid)
        {
            try
            {
                TotalA3AndKaizenViewModel rtn = new TotalA3AndKaizenViewModel();
                var A3Total = (from a in _context.TrainingEventAttendees
                               join c in _context.TrainingEvents on a.TrainigEventID equals c.ID
                               where a.EmployeeID == new Guid(EmployeeID) && c.Isclosed == true && c.TrainingEventFormatID == new Guid("6F9F04CC-198E-479C-A93F-6C3C0A359194")
                               select c).Count();

                var KaizenTotal = (from a in _context.TrainingEventAttendees
                                   join c in _context.TrainingEvents on a.TrainigEventID equals c.ID
                                   where a.EmployeeID == new Guid(EmployeeID) && c.Isclosed == true && c.TrainingEventFormatID == new Guid("5518993A-EFC0-4AD0-BCD7-BEAEA42CC2CE")
                                   select c).Count();
                rtn.TotalA3 = A3Total;
                rtn.TotalKaizen = KaizenTotal;
                if (_context.CreditLogs.Any(d => d.UserDetailID == new Guid(userdetailid)))
                {
                    rtn.TotalCredits = _context.CreditLogs.Where(d => d.UserDetailID == new Guid(userdetailid)).Sum(d => d.Credit);
                }
                else
                {
                    rtn.TotalCredits = 0;
                }

                return rtn;
            }
            catch (Exception ex)
            {
                return new TotalA3AndKaizenViewModel { TotalA3 = 0, TotalCredits = 0, TotalKaizen = 0 };
            }
        }
        public int getBeltID(decimal credit, int A3, int Kaizen, string companyId, string userDetailID)
        {
            List<BeltRule> achievedBelts = new List<BeltRule>();
            if (!string.IsNullOrWhiteSpace(companyId) && new Guid(companyId) != Guid.Empty)
                achievedBelts = _context.BeltRules.Where(r => r.TotalCredit <= credit && r.TotalA3 <= A3 && r.TotalKaizen <= Kaizen && r.IsActive == true &&
                    r.CompanyID == new Guid(companyId)).OrderByDescending(d => d.OrderValue).ToList();
            else
                achievedBelts = _context.BeltRules.Where(r => r.TotalCredit <= credit && r.TotalA3 <= A3 && r.TotalKaizen <= Kaizen && r.IsActive == true &&
                        r.UserDetailID == new Guid(userDetailID)).OrderByDescending(d => d.OrderValue).ToList();

            if (achievedBelts.Count > 0)
            {
                return achievedBelts[0].ID;
            }
            else
            {
                return -1;
            }
        }

        public string deleteBeltRules(WorkQueue workQueue)
        {            
            try
            {
                if (workQueue != null)
                {
                    BeltRule beltrules = new JavaScriptSerializer().Deserialize<BeltRule>(workQueue.WorkData);
                    var comp = _context.CompanyDetails.ToList();
                    foreach (var items in comp)
                    {
                        var belts = _context.BeltRules.FirstOrDefault(b => b.ParentID == beltrules.ID && b.CompanyID == items.ID);
                        var res = _context.BeltAchievementLogs.Any(d => d.BeltRuleID == belts.ID && d.CompanyID == items.ID);
                        if (!res)
                        {
                            var del = _context.BeltRules.FirstOrDefault(d => d.ID == belts.ID && d.CompanyID == items.ID);
                            if (del != null)
                            {
                                del.IsActive = false;
                                del.ModifiedDate = DateTime.UtcNow;
                                _context.SaveChanges();
                            }
                           
                        }
                    }

                    var individuals = _context.UserDetails.Where(d => d.UserType == new Guid("FBDE320E-6619-4F25-9E7F-2FCC94D2879E")).ToList();
                    foreach (var items in individuals)
                    {
                        var belts = _context.BeltRules.FirstOrDefault(b => b.ParentID == beltrules.ID && b.UserDetailID == items.ID);
                        var res = _context.BeltAchievementLogs.Any(d => d.BeltRuleID == belts.ID && d.UserDetailID == items.ID);
                        if (!res)
                        {
                            var del = _context.BeltRules.FirstOrDefault(d => d.ID == belts.ID && d.UserDetailID == items.ID);
                            if (del != null)
                            {
                                del.IsActive = false;
                                del.ModifiedDate = DateTime.UtcNow;
                                _context.SaveChanges();
                            }
                        }
                    }
                }
                return WorkItemStatus.Completed.ToString();
            }
            catch (Exception)
            {
                return WorkItemStatus.Failed.ToString();
            }
            
        }
    }
}
