using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel.TrainingEvents
{
    public class KaizenFormViewModel
    {
        public int? id { get; set; }
        public string TrainingEventID { get; set; }
        public string DefineTheProblem { get; set; }
        public string ImplementationPlan { get; set; }
        public string CurrentCondition { get; set; }
        public string ActionItemTimeline { get; set; }
        public string Goal { get; set; }
        public string FollowUp { get; set; }
        public string Analysis { get; set; }
        public string UserID { get; set; }
        public string mentorEmail { get; set; }
        public string mentorName { get; set; }
        public decimal DollarImpacted { get; set; }
        public string AssignedTo { get; set; }
    }
}
