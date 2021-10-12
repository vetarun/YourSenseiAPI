using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel.TrainingEvents
{
    public class A3TrainingEventViewModel
    {
        public Guid TrainingEventID { get; set; }
        public string IconUrl { get; set; }
        public string Name { get; set; }
        public string InstructorName { get; set; }
        public DateTime startDate { get; set; }
        public string Status { get; set; }
    }
}
