using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel
{
    public class AssessmentAnswerInputViewModel
    {
        public int questionid { get; set; }
        public int optionid { get; set; }
        public string userdetailid { get; set; }
        public string companyid { get; set; }       
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public int score { get; set; }
    }
}
