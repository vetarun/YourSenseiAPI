using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel
{
  public  class SubscriptionPlanViewModel : ResponseViewModel
    {
        public int ID { get; set; }
        public string PlanName { get; set; }
        public string Description { get; set; }
        public int NumberOfDays { get; set; }
        public int NumberOfEmployees { get; set; }
        public int NumberOfExternalMentors { get; set; }
        public decimal Price { get; set; }
        public string FeaturesAllowed { get; set; }
        public string[] FeaturesAllowedArray { get; set; }
        public bool IsActive { get; set; }
        public bool IsTrialPlan { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public List<string> FeaturesAllowedList { get; set; }
        public string FinalFeaturesAllowed { get; set; }
    }
}
