using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourSensei.Data;
using YourSensei.ViewModel;
using YourSensei.ViewModel.Subscription;

namespace YourSensei.Service
{
    public interface ISubscriptionService
    {
        Task<List<SubscriptionPlanViewModel>> GetSubscriptionPlans(int id);
        Task<SubscriptionResponseViewModel> SubscribePlanToUser(string userdetailid, string companyid, int planID, int noOfDays);
        Task<List<SubscriptionViewModel>> GetSubscriptions();
        Task<ResponseViewModel> addUpdateSubscriptionPlan(SubscriptionPlanViewModel obj);
        Task<FeaturesModel> GetFeaturesAllowed();
        Task<SubscriptionPlan> GetSubscribedPlan(Guid companyID, Guid userDetailID);
        Task<Subscription> GetSubscriptionsByCompanyIDAndPlanID(Guid? companyID, int planID);
        Task<ResponseViewModel> Save(Subscription subscription);
        Task<Subscription> GetSubscriptionsByUserDetailIDAndPlanID(Guid? userDetailID, int planID);
        Task<List<Subscription>> GetSubscriptionsByIsActivatedAndIsExpired(bool isActivated, bool isExpired);
        ResponseViewModel SavePayPalTransaction(SubscriptionTransaction obj);
    }
}
