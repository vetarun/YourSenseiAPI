using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel
{
    public class SelectEmployeeToEventResponseViewModel
    {
        public Guid TrainingEventAttendeeID { get; set; }
        public Guid EmpId { get; set; }
        public string EmployeeName { get; set; }
        public string EventTrainingName { get; set; }
        public string Duration { get; set; }
        public DateTime ScheduleDate { get; set; }
        public decimal? Time { get; set; }
        public decimal? Test { get; set; }
        public Boolean Isselected { get; set; }
    }
}
