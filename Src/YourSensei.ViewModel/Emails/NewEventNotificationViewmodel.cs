using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel
{
    public class NewEventNotificationViewmodel
    {
        public string ToEmployeeEmail { get; set; }

        public string ToEmployeeName { get; set; }
        public string TrainingEventName { get; set; }

        public string TrainingEventCreator { get; set; }

        public string Instructor { get; set; }

        public string ScheduledDate { get; set; }

        public string TrainingNotes { get; set; }

        public string StudentsInvited { get; set; }

    }
}
