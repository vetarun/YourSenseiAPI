using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using YourSensei.Data;
using YourSensei.Service;
using YourSensei.Utility;

namespace YourSensei.WorkQueueProcessor
{
    public class SubscriptionHandler : ISubscriptionHandler
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionHandler(SubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        public string ProcessRenewRequest(WorkQueue workQueue)
        {
            Subscription newSubscription = new JavaScriptSerializer().Deserialize<Subscription>(workQueue.WorkData);
            if (newSubscription != null)
            {
                Task<Subscription> taskSubscription = null;
                if (workQueue.CompanyID != null)
                    taskSubscription = Task.Run(() => _subscriptionService.GetSubscriptionsByCompanyIDAndPlanID(workQueue.CompanyID,
                        newSubscription.PlanID));
                else if (workQueue.UserDetailID != null)
                    taskSubscription = Task.Run(() => _subscriptionService.GetSubscriptionsByUserDetailIDAndPlanID(workQueue.CompanyID,
                        newSubscription.PlanID));
                else
                    return WorkItemStatus.Failed.ToString();

                taskSubscription.Wait();
                Subscription subscription = taskSubscription.Result;
                if (subscription != null) { 
                if (Convert.ToDateTime(subscription.ExpiryDate).Date < DateTime.UtcNow)
                {
                    subscription.IsActivated = false;
                    subscription.IsExpired = true;
                    subscription.ModifiedDate = DateTime.UtcNow;
                    _subscriptionService.Save(subscription);

                    newSubscription.ID = 0;
                    newSubscription.IsActivated = true;
                    newSubscription.IsExpired = false;
                    newSubscription.ModifiedDate = DateTime.UtcNow;
                    _subscriptionService.Save(newSubscription);

                    return WorkItemStatus.Completed.ToString();
                }
                }
                return WorkItemStatus.Pending.ToString();
            }
            return WorkItemStatus.Failed.ToString();
        }

        public void ProcessingExpirydate()
        {
            try
            {
                Task<List<Subscription>> taskSubscriptionList = Task.Run(() => _subscriptionService.GetSubscriptionsByIsActivatedAndIsExpired(true, false));
                taskSubscriptionList.Wait();
                List<Subscription> subscriptions = taskSubscriptionList.Result;

                foreach (Subscription subscription in subscriptions)
                {
                    if (Convert.ToDateTime(subscription.ExpiryDate).Date < DateTime.UtcNow)
                    {
                        subscription.IsActivated = false;
                        subscription.IsExpired = true;
                        _subscriptionService.Save(subscription);
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
