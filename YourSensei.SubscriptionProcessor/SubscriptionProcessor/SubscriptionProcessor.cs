using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourSensei.Data;
using YourSensei.Service;

namespace YourSensei.SubscriptionProcessor
{
    public class SubscriptionProcessor : ISubscriptionProcessor
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionProcessor(SubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        public void process()
        {
            Task<List<Subscription>> taskSubscriptionList = Task.Run(() => _subscriptionService.GetSubscriptionsByIsActivatedAndIsExpired(true, false));
            taskSubscriptionList.Wait();
            List<Subscription> subscriptions = taskSubscriptionList.Result;
            foreach (Subscription subscription in subscriptions)
            {
                if (Convert.ToDateTime(subscription.ExpiryDate).Date < DateTime.UtcNow.Date)
                {

                }
            }
        }
    }
}
