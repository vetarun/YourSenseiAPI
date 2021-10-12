using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel
{
    public class QuestionViewModel
    {
        public int quizID { get; set; }
        public int quesID { get; set; }
        public string questionType { get; set; }
        public string questionName { get; set; }
        public string correctAnswer { get; set; }
        public string option1 { get; set; }
        public string option2 { get; set; }
        public string option3 { get; set; }
        public string option4 { get; set; }
        public string userDetailID { get; set; }
    }
}
