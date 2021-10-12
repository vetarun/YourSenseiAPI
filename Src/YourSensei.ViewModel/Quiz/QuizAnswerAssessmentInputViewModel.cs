using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel
{
    public class QuizAnswerAssessmentInputViewModel
    {
        public int quizid { get; set; }
        public int questionid { get; set; }
        public string userdetailid { get; set; }
        public string companyid { get; set; }
        public string defaultanswer { get; set; }
        public string useranswer { get; set; }
        public string bookid { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string EmployeeID { get; set; }
        public string QuizName { get; set; }
        public string QuestionName { get; set; }
        public int QuestionOptionID { get; set; }
        public string Options { get; set; }
        public DateTime QuizDate { get; set; }
        public string QuestionType { get; set; }
        public string CompanyName { get; set; }
        public string Title { get; set; }
    }
}
