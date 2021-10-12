using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourSensei.Data;

namespace YourSensei.ViewModel
{
    public class GetQuestionListResponseViewModel
    {
        public int quizid { get; set; }
        public int questionid { get; set; }
        public string question { get; set; }
        public string questiontype { get; set; }
        public string defaultans { get; set; }
        public List<QuestionOption> questionoptions { get; set; }
        public string useranswer { get; set; }
        public string instruction { get; set; }
    }
}
