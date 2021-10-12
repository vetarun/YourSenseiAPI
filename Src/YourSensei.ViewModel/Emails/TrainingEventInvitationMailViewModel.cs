using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel
{
    public class TrainingEventInvitationMailViewModel
    {
        public string FullName { get; set; }
        public string TrainingEventCreator { get; set; }
        public string TrainingEventName { get; set; }
        public string Instructor { get; set; }
        public DateTime ScheduleDate { get; set; }
        public string TrainingNote { get; set; }
        public string EmployeeEmail { get; set; }
    }
}
