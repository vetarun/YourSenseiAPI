using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourSensei.Data;

namespace YourSensei.ViewModel
{
    public class GetQuizLIstResponseViewModel
    {
        public int quizid { get; set; }
        public string quizname { get; set; }
        public string descrition { get; set; }
        public bool isactive { get; set; }
        public string bookname { get; set; }
        public bool ispublished { get; set; }
        public List<Question> questionlist { get; set; }
        
    }

   
}
