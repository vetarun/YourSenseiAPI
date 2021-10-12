using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourSensei.Data;
using YourSensei.ViewModel;
using YourSensei.ViewModel.Dashboard;

namespace YourSensei.Service
{
    public interface IDashboardService
    {
        Task<List<PyramidViewModel>> GetBeltList(int id, string companyId, string UserId);
        Task<ResponseViewModel> AddUpdateBelt(BeltRuleInputViewModel belt);
        Task<ResponseViewModel> DeleteBelt(int id, string userid);
        Task<GetDashboardBeltRuleViewModel> GetDashboardBeltDetails(string companyId, string UserId, string employeeID, bool mentor);
        void ReplicateBeltForCompanyIndividual(Guid? companyID, Guid? userID);
        Task<List<CreditStandingViewModel>> GetCreditStandings(string companyID, string UserDetailID,bool IsMentor);
        BeltPropertiesViewModel getBeltName(decimal credit, int A3, int Kaizen, string companyId, string userDetailID);
    }
}
