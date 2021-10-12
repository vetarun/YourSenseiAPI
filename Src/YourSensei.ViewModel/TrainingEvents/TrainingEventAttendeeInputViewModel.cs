using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel
{
    public class TrainingEventAttendeeInputViewModel
    {
        public string TrainigEventID { get; set; }
        public string trainingEventAttendeeID { get; set; }
        public string EmployeeID { get; set; }
        public decimal? Time { get; set; }
        public decimal? Test { get; set; }
       
    }
}
