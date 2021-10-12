using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel.Subscription
{
    public class FeaturesAllowedModel
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
    public class FeaturesModel
    {
        public bool IsTrailPlanAlreadyExist { get; set; }
        public List<FeaturesAllowedModel> FeaturesAllowed { get; set;}
    }

}
