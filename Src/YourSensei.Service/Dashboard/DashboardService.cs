using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using YourSensei.Data;
using YourSensei.Utility;
using YourSensei.ViewModel;
using YourSensei.ViewModel.Dashboard;


namespace YourSensei.Service
{
    public class DashboardService : IDashboardService
    {
        private readonly YourSensei_DBEntities _context;
        private readonly ICreditLogService _creditService;
        private readonly IWorkQueueService _workQueueService;
        public DashboardService(WorkQueueService workQueueService, YourSensei_DBEntities context, CreditLogService creditdervice)
        {
            _context = context;
            _creditService = creditdervice;
            _workQueueService = workQueueService;

        }
        public async Task<List<PyramidViewModel>> GetBeltList(int id, string companyId, string UserId)
        {
            try
            {
                
                List<BeltRule> beltrulelist;
                if (!string.IsNullOrWhiteSpace(companyId))
                {
                    beltrulelist = await (from sp in _context.BeltRules
                                          where sp.IsActive == true && ((sp.ID == id && sp.CompanyID == new Guid(companyId)) || (id == 0 && sp.CompanyID == new Guid(companyId)))
                                          select sp).OrderBy(x => x.OrderValue).ToListAsync();
                }
                else
                {
                    beltrulelist = await (from sp in _context.BeltRules
                                          where sp.IsActive == true && ((sp.ID == id && sp.UserDetailID == new Guid(UserId)) || (id == 0 && sp.UserDetailID == new Guid(UserId)))
                                          select sp).OrderBy(x => x.OrderValue).ToListAsync();
                }
               
                List<PyramidViewModel> lst = new List<PyramidViewModel>();
                foreach(var belt in beltrulelist)
                {
                    
                    lst.Add(new PyramidViewModel
                    {
                        ProjectedDate= "",
                        BeltColor=belt.BeltColor,
                        BeltName=belt.BeltName,
                        CompanyID=belt.CompanyID,
                        CreateDate=belt.CreateDate,
                        CreatedBy=belt.CreatedBy,
                        ID=belt.ID,
                        IsActive=belt.IsActive,
                        ModifiedBy=belt.ModifiedBy,
                        ModifiedDate=belt.ModifiedDate,
                        OrderValue=belt.OrderValue,
                        ParentID=belt.ParentID,
                        TotalA3=belt.TotalA3,
                        TotalCredit=belt.TotalCredit,
                        TotalKaizen=belt.TotalKaizen,
                        UserDetailID=belt.UserDetailID

                    });
                }
                return lst;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public async Task<ResponseViewModel> AddUpdateBelt(BeltRuleInputViewModel belt)
        {
            BeltRule obj = new BeltRule();
            int ordernumber = 0;
            if (belt.CompanyID == "00000000-0000-0000-0000-000000000000")
                ordernumber = await _context.BeltRules.Where(x => x.CompanyID == new Guid(belt.CompanyID)).CountAsync();


            try
            {
                if (belt.ID == 0)
                {
                    var result = await _context.BeltRules.AnyAsync(d => d.BeltName == belt.BeltName && d.IsActive == true);
                    if (result == false)
                    {
                        obj = new BeltRule();
                        obj.IsActive = true;
                        obj.BeltColor = belt.BeltColor;
                        if (belt.isIndividual)
                        {
                            obj.UserDetailID = new Guid(belt.UserDetailID);
                        }
                        else
                        {
                            obj.CompanyID = new Guid(belt.CompanyID);
                        }
                        obj.BeltName = belt.BeltName;
                        obj.TotalA3 = belt.TotalA3;
                        obj.TotalCredit = belt.TotalCredit;
                        obj.TotalKaizen = belt.TotalKaizen;
                        obj.CreatedBy = new Guid(belt.CreatedBy);
                        obj.CreateDate = DateTime.UtcNow;
                        obj.OrderValue = ordernumber + 1;
                        var response = _context.BeltRules.Add(obj);

                        await _context.SaveChangesAsync();

                        if (belt.CompanyID == "00000000-0000-0000-0000-000000000000" && !belt.isIndividual)
                        {
                            ReplicateGlobalBeltRule(obj, "add");
                        }


                        return new ResponseViewModel { Code = 200, Message = "Your BeltRule has been created successfully" };
                    }
                    else
                    {
                        return new ResponseViewModel { Code = 201, Message = "Belt name is already exist" };
                    }

                }
                else
                {

                    var result = await _context.BeltRules.FirstOrDefaultAsync(d => d.ID == belt.ID);
                    if (result != null)
                    {
                        //result.CompanyID = new Guid(belt.CompanyID);
                        result.BeltColor = belt.BeltColor;
                        result.BeltName = belt.BeltName;
                        result.TotalA3 = belt.TotalA3;
                        result.TotalCredit = belt.TotalCredit;
                        result.TotalKaizen = belt.TotalKaizen;
                        result.ModifiedDate = DateTime.UtcNow;
                        result.ModifiedBy = new Guid(belt.ModifiedBy);

                        var response = await _context.SaveChangesAsync();

                        return new ResponseViewModel { Code = 200, Message = "Your Belt Rule has been updated successfully" };
                    }

                    return new ResponseViewModel { Code = 400, Message = "Something went wrong please try after a moment" };
                }
            }
            catch (Exception ex)
            {
                return new ResponseViewModel { Code = 400, Message = "Something went wrong please try after a moment" };
            }
        }

        private void ReplicateGlobalBeltRule(BeltRule belt, string v)
        {
            try
            {
                BeltRule obj = null;
                var companyList = _context.CompanyDetails.ToList();
                string userTypeDescription = EnumHelper.GetDescription(Utility.UserCategory.Individual);
                Guid userTypeID = _context.UserCategories.Where(a => a.Description == userTypeDescription).Select(d => d.Id).SingleOrDefault();
                var individuallist = _context.UserDetails.Where(x => x.UserType == userTypeID).ToList();
                foreach (var item in companyList)
                {
                    obj = new BeltRule();
                    obj.IsActive = true;
                    obj.BeltColor = belt.BeltColor;
                    obj.CompanyID = item.ID;
                    obj.BeltName = belt.BeltName;
                    obj.TotalA3 = belt.TotalA3;
                    obj.TotalCredit = belt.TotalCredit;
                    obj.TotalKaizen = belt.TotalKaizen;
                    obj.OrderValue = belt.OrderValue;
                    obj.CreatedBy = belt.CreatedBy;
                    obj.CreateDate = DateTime.UtcNow;
                    obj.ParentID = belt.ID;
                    _context.BeltRules.Add(obj);
                    _context.SaveChanges();
                }
                foreach (var item in individuallist)
                {
                    obj = new BeltRule();
                    obj.IsActive = true;
                    obj.BeltColor = belt.BeltColor;
                    obj.UserDetailID = item.ID;
                    obj.BeltName = belt.BeltName;
                    obj.TotalA3 = belt.TotalA3;
                    obj.TotalCredit = belt.TotalCredit;
                    obj.TotalKaizen = belt.TotalKaizen;
                    obj.CreatedBy = belt.CreatedBy;
                    obj.OrderValue = belt.OrderValue;
                    obj.CreateDate = DateTime.UtcNow;
                    obj.ParentID = belt.ID;
                    _context.BeltRules.Add(obj);
                    _context.SaveChanges();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void ReplicateBeltForCompanyIndividual(Guid? companyID, Guid? userID)
        {
            var globalBeltList = _context.BeltRules.Where(d => d.IsActive == true && d.CompanyID == new Guid("00000000-0000-0000-0000-000000000000")).ToList();
            List<BeltRule> obj = new List<BeltRule>();
            foreach (var item in globalBeltList)
            {
                _context.BeltRules.Add(new BeltRule
                {
                    IsActive = item.IsActive,
                    BeltColor = item.BeltColor,
                    CompanyID = companyID == null || companyID == Guid.Empty ? null : companyID,
                    UserDetailID = companyID != null && companyID != Guid.Empty ? null : userID,
                    BeltName = item.BeltName,
                    TotalA3 = item.TotalA3,
                    TotalCredit = item.TotalCredit,
                    TotalKaizen = item.TotalKaizen,
                    OrderValue = item.OrderValue,
                    CreatedBy = item.CreatedBy,
                    CreateDate = DateTime.UtcNow,
                    ParentID = item.ID,

                });
            }
            _context.SaveChanges();
        }



        public async Task<GetDashboardBeltRuleViewModel> GetDashboardBeltDetails(string companyId, string UserId, string employeeID,bool ismentor)
        {
            try
            {
                List<PyramidViewModel> beltlist = new List<PyramidViewModel>();
                List<BeltRule> achievedBelts = new List<BeltRule>();
                GetDashboardBeltRuleViewModel mod = new GetDashboardBeltRuleViewModel();

                var logs = await _creditService.GetCreditLogsByLoggedInUser(UserId, true);
                mod.totalcredit = logs.SumOfCredits;
                var A3Total = (from a in _context.TrainingEventAttendees
                               join c in _context.TrainingEvents on a.TrainigEventID equals c.ID
                               where a.EmployeeID == new Guid(employeeID) && c.Isclosed == true &&
                                c.TrainingEventFormatID == new Guid("6F9F04CC-198E-479C-A93F-6C3C0A359194")
                               select c).Count();

                var KAizenTotal = (from a in _context.TrainingEventAttendees
                                   join c in _context.TrainingEvents on a.TrainigEventID equals c.ID
                                   where a.EmployeeID == new Guid(employeeID) && c.Isclosed == true &&
                                    c.TrainingEventFormatID == new Guid("5518993A-EFC0-4AD0-BCD7-BEAEA42CC2CE")
                                   select c).Count();

                if ((!string.IsNullOrWhiteSpace(companyId) && new Guid(companyId) != Guid.Empty) || ismentor == true)
                {
                    beltlist = await GetBeltList(0, companyId, string.Empty);
                    achievedBelts = _context.BeltRules.Where(r => r.TotalCredit <= logs.SumOfCredits && r.TotalA3 <= A3Total &&
                        r.TotalKaizen <= KAizenTotal && r.CompanyID == new Guid(companyId) &&
                        r.IsActive == true).OrderByDescending(d => d.OrderValue).ToList();
                }
                else
                {
                    beltlist = await GetBeltList(0, string.Empty, UserId);
                    achievedBelts = _context.BeltRules.Where(r => r.TotalCredit <= logs.SumOfCredits && r.TotalA3 <= A3Total &&
                        r.TotalKaizen <= KAizenTotal && r.UserDetailID == new Guid(UserId) &&
                        r.IsActive == true).OrderByDescending(d => d.OrderValue).ToList();

                }
                if (!ismentor)
                {
                    List<CreditLog> pd = _context.CreditLogs.Where(d => d.UserDetailID == new Guid(UserId)).OrderBy(d => d.AwardedDate).ToList();
                    DateTime AwardedDate = pd == null || pd.Count == 0 ? DateTime.MinValue : pd[0].AwardedDate;
                    int datediff = 0;
                    List<int> arrstr = new List<int>();
                    int index = 0;
                    foreach (var belts in beltlist)
                    {
                        if (index > 0)
                        {
                            var result = _context.BeltAchievementLogs.FirstOrDefault(d => d.UserDetailID == new Guid(UserId) && d.BeltRuleID == belts.ID);
                            if (result == null)
                            {

                                AwardedDate = AwardedDate.AddDays(arrstr.Count == 0 ? 30 : arrstr.Sum() / arrstr.Count);
                                belts.ProjectedDate = "Proj. " + AwardedDate.ToString("MM/dd/yyyy");
                            }
                            else
                            {
                                belts.ProjectedDate = "Earned " + result.AchievedDate.ToString("MM/dd/yyyy");

                                datediff = Convert.ToInt32((result.AchievedDate - AwardedDate).TotalDays);
                                arrstr.Add(datediff);
                                AwardedDate = result.AchievedDate;
                            }
                        }
                        else
                        {
                            var result = _context.BeltAchievementLogs.FirstOrDefault(d => d.UserDetailID == new Guid(UserId) && d.BeltRuleID == belts.ID);
                            if (result != null)
                            {
                                belts.ProjectedDate = "Earned " + result.AchievedDate.ToString("MM/dd/yyyy") + " ";
                                datediff = Convert.ToInt32((result.AchievedDate - AwardedDate).TotalDays);
                                arrstr.Add(datediff);
                                belts.ProjectedDate += "Started " + AwardedDate.ToString("MM/dd/yyyy");
                                AwardedDate = result.AchievedDate;
                            }
                            else
                            {
                                DateTime date = AwardedDate.AddDays(30);
                                belts.ProjectedDate = "Proj. " + date.ToString("MM/dd/yyyy") + " ";
                                belts.ProjectedDate += "Started " + AwardedDate.ToString("MM/dd/yyyy");
                                AwardedDate = date;
                            }


                        }

                        index++;
                    }
                }

                mod.ListOfBelt = beltlist.OrderByDescending(x => x.OrderValue).ToList();

                if (!ismentor)
                {
                    if (achievedBelts.Count() == 0)
                    {
                        mod.indexofyouarehere = mod.ListOfBelt.Count() - 1;



                    }
                    else
                    {
                        int YouAreHere = mod.ListOfBelt.IndexOf(mod.ListOfBelt.Where(a => a.OrderValue == achievedBelts[0].OrderValue).FirstOrDefault());
                        mod.indexofyouarehere = YouAreHere - 1;





                    }
                    if (mod.indexofyouarehere < 0)
                    {
                        mod.creditwant = 0;
                        mod.a3want = 0;
                        mod.kaizenwant = 0;
                    }
                    else
                    {
                        mod.creditwant = mod.ListOfBelt[mod.indexofyouarehere].TotalCredit - logs.SumOfCredits;
                        mod.a3want = mod.ListOfBelt[mod.indexofyouarehere].TotalA3 - A3Total;
                        mod.kaizenwant = mod.ListOfBelt[mod.indexofyouarehere].TotalKaizen - KAizenTotal;
                    }
                }
                
                return mod;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<ResponseViewModel> DeleteBelt(int id, string userid)
        {
            try
            {
                var isBeltExist = await _context.BeltRules.FindAsync(id);
                if (isBeltExist != null)
                {
                    isBeltExist.IsActive = false;
                    isBeltExist.ModifiedBy = new Guid(userid);
                    isBeltExist.ModifiedDate = DateTime.UtcNow;
                    _context.SaveChanges();
                    var workqueue = new WorkQueue
                    {
                        CompanyID=null,
                        UserDetailID=null,
                        WorkData = new JavaScriptSerializer().Serialize(isBeltExist),
                        Status = "Pending",
                        CreatedDate = DateTime.UtcNow,
                        ModifiedDate = DateTime.UtcNow,
                        WorkItemType = "DeleteBeltRule"
                    };
                    await _workQueueService.Save(workqueue);
                    return new ResponseViewModel { Code = 200, Message = "Belt successfully deleted!" };
                }
                else
                {
                    return new ResponseViewModel { Code = 201, Message = "Belt not found!" };
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<CreditStandingViewModel>> GetCreditStandings(string companyID, string UserDetailID,bool IsMentor)
        {
            try
            {
                List<CreditStandingViewModel> emptycreditslist = new List<CreditStandingViewModel>();
                Guid CompanyID = new Guid(companyID);
                Guid userDetailID = new Guid(UserDetailID);
                List<CreditStandingViewModel> creditStanding = await _context.Database.SqlQuery<CreditStandingViewModel>(
                    "dbo.usp_GetCreditStandings @CompanyID = @companyID, @UserDetailID = @userDetailID,@IsMentor=@isMentor",
                    new SqlParameter("companyID", CompanyID),
                    new SqlParameter("userDetailID", userDetailID),
                    new SqlParameter("isMentor", IsMentor)).ToListAsync();

                foreach (var item in creditStanding)
                {
                    calculateLastWeekCredits(ref emptycreditslist, creditStanding, Convert.ToString(item.UserDetailID), companyID, item.EmployeeID);
                }

                return emptycreditslist.OrderByDescending(d=> d.CICredits).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void calculateLastWeekCredits(ref List<CreditStandingViewModel> emptycreditslist, List<CreditStandingViewModel> creditslist, string userdetailid, string companyId, Guid EmployeeID)
        {
            decimal rtn = 0;
            DateTime date = DateTime.UtcNow;
            DateTime mondayOfLastWeek = date.AddDays(-(int)date.DayOfWeek - 6);
            DateTime saturdayOfLastWeek = mondayOfLastWeek.AddDays(6);


            if (!emptycreditslist.Any(d => d.UserDetailID == new Guid(userdetailid)))
            {

                var A3Total = (from a in _context.TrainingEventAttendees
                               join c in _context.TrainingEvents on a.TrainigEventID equals c.ID
                               where a.EmployeeID == EmployeeID && c.Isclosed == true && c.TrainingEventFormatID == new Guid("6F9F04CC-198E-479C-A93F-6C3C0A359194")
                               select c).Count();

                var KAizenTotal = (from a in _context.TrainingEventAttendees
                                   join c in _context.TrainingEvents on a.TrainigEventID equals c.ID
                                   where a.EmployeeID == EmployeeID && c.Isclosed == true && c.TrainingEventFormatID == new Guid("5518993A-EFC0-4AD0-BCD7-BEAEA42CC2CE")
                                   select c).Count();


                var result = creditslist.FirstOrDefault(d => (d.AwardedDate.Date >= mondayOfLastWeek.Date || d.AwardedDate.Date <= saturdayOfLastWeek.Date) && d.UserDetailID == new Guid(userdetailid));
                decimal lastweekci = creditslist.Where(d => (d.AwardedDate.Date >= mondayOfLastWeek.Date && d.AwardedDate.Date <= saturdayOfLastWeek.Date) && d.UserDetailID == new Guid(userdetailid)).Sum(d => d.CICredits);
                result.CILastWeek = lastweekci;
                result.CICredits = creditslist.Where(d => d.UserDetailID == new Guid(userdetailid)).Sum(d => d.CICredits);
                result.BeltName = getBeltName(result.CICredits, A3Total, KAizenTotal, companyId, userdetailid).BeltName;
                result.BeltColor = getBeltName(result.CICredits, A3Total, KAizenTotal, companyId, userdetailid).BeltColor;
                //result.ProjectedDate = _context.BeltAchievementLogs.Any(d => d.UserDetailID == new Guid(userdetailid)) ? _context.BeltAchievementLogs.FirstOrDefault(d => d.UserDetailID == new Guid(userdetailid)).AchievedDate.ToString() : "";
                emptycreditslist.Add(result);

            }
        }

        public BeltPropertiesViewModel getBeltName(decimal credit, int A3, int Kaizen, string companyId, string userDetailID)
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
                return new BeltPropertiesViewModel { BeltName = achievedBelts[0].BeltName, BeltColor = achievedBelts[0].BeltColor };
            }
            else
            {
                return new BeltPropertiesViewModel { BeltName = "", BeltColor = "" };
            }
        }

        
    }


}
