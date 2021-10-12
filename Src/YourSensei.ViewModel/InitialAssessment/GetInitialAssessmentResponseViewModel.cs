using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourSensei.Data;

namespace YourSensei.ViewModel
{
    public class GetInitialAssessmentResponseViewModel
    {
        public int catid { get; set; }
        public string catname { get; set; }
        public int questionid { get; set; }
        public string question { get; set; }
        public string questiontype { get; set; }
        public List<InitialAssessmentOption> questionoptions { get; set; }
    }
}
