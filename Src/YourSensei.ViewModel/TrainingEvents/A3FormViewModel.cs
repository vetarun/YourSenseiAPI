using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel
{
    public class A3FormViewModel
    {
        public int? id { get; set; }
        public string TrainingEventID { get; set; }
        public string Background { get; set; }
        public string Proposal { get; set; }
        public string CurrentCondition { get; set; }
        public string Plan { get; set; }
        public string Goal { get; set; }
        public string FollowUp { get; set; }
        public string Analyses { get; set; }
        public string userid { get; set; }
        public string mentorEmail { get; set; }
        public string mentorName { get; set; }
        public decimal DollarImpacted { get; set; }
        public string AssignedTo{ get; set; }
    }
}
