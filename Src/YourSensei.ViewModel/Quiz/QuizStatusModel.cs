using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel
{
    public class QuizStatusModel
    {
        public int ID { get; set; }
        public Guid UserdetailID { get; set; }
        public Guid CompanyID { get; set; }
        public Guid CompanyLibraryBookID { get; set; }
        public int QuizID { get; set; }
        public bool IsQuizStarted { get; set; }
        public bool IsQuizFinished { get; set; }
        public string QuizName { get; set; }
        public string BookName { get; set; }

    }
}
