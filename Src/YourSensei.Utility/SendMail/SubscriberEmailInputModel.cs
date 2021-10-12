using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.Utility
{
    public class SubscriberEmailInputModel
    {
         public string SubscriberEmail { get; set; }
        public string SubscriberName { get; set; }
        public string PlanName { get; set; }
        public DateTime ActivationDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int TotalNumberOfPlanDays { get; set; }
        public int TotalNumberOfEmployees { get; set; }
        public int TotalNumberOfMentor { get; set; }
        public string AccessableFeatures { get; set; }
    }
}
