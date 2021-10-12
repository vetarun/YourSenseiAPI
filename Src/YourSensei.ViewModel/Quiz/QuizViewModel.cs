using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel
{
    public class QuizViewModel
    {
        public int quizid { get; set; }
        public string quizName { get; set; }
        public string description { get; set; }
        public string bookID { get; set; }
        public string bookTitle { get; set; }
        public string userDetailID { get; set; }
        public string Instructions { get; set; }
    }
}
