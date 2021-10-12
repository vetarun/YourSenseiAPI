using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel
{
    public class CreateEventInputViewModel
    {
        public string id { get; set; }
        public string owner { get; set; }
        public string responsibletrainer { get; set; }
        //public DateTime plannedtrainingdate { get; set; }
        public string trainingformat { get; set; }
        public string trainingdescription { get; set; }
        public string trainingnotes { get; set; }
        public string bannerimageurl { get; set; }
        public string eventsname { get; set; }
        public string location { get; set; }
        public string instructor { get; set; }
        public string duration { get; set; }
        public DateTime startdate { get; set; }
        //public DateTime enddate { get; set; }
        public string companyid { get; set; }
        public string userDetailedID { get; set; }
        public bool isIndividual { get; set; }
        public string ClosingNote { get; set; }
        public Boolean IsClosed { get; set; }
        public string responsibleTrainerName { get; set; }
        public string trainingFormatName { get; set; }
        public string instructorName { get; set; }
        public DateTime? ClosedDate { get; set; }
        public string status { get; set; }
    }
}
