using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel
{
    public class SendKaizenFormUpdateNotifyEmail
    {
        public string receivername { get; set; }
        public string receiverEmail { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string DefineTheProblem { get; set; }
        public string Condition { get; set; }
        public string Analyses { get; set; }
        public string FollowUp { get; set; }
        public string Goal { get; set; }
        public string Plan { get; set; }
        public string ActionItemTimeline { get; set; }
        public string DollarImpacted { get; set; }
        public string url { get; set; }
    }
}
