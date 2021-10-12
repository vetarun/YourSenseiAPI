using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourSensei.Data;

namespace YourSensei.ViewModel
{
    public class InitialAssessmentAnswerViewModel
    {
        public int SequenceNumber { get; set; }
        public string CompanyName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool Registered { get; set; }
        public string QuestionName { get; set; }
        public string OptionValue { get; set; }
        public int Score { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
