using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel
{
    public class TrainingEventListViewModel
    {
        public Guid Id { get; set; }
        public string IconUrl { get; set; }
        public string EventsName { get; set; }
        public string EventFormat { get; set; }
        public string Instructor { get; set; }
        public string Duration { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; }
        public Guid Owner { get; set; }
        public string status { get; set; }
    }
}
