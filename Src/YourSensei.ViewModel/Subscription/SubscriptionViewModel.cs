using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel
{
  public  class SubscriptionViewModel
    {
        public int SubscriptionID { get; set; }
        public int PlanID { get; set; }
        public string PlanName { get; set; }
        public string SubscriptionOwner { get; set; }
        public System.DateTime PurchasedDate { get; set; }
        public string PurchasedBy { get; set; }
        public Nullable<System.DateTime> RenewalDate { get; set; }
        public System.DateTime ActivationDate { get; set; }
        public Nullable<System.DateTime> ExpiryDate { get; set; }
        public bool IsExpired { get; set; }
    }
}
