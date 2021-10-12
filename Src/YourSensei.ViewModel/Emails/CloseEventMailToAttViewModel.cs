using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel
{
    public class CloseEventMailToAttViewModel
    {
        public string FullName { get; set; }
        public string EventName { get; set; }
        public string EventCreator { get; set; }
        public string ToEmail { get; set; }
        public decimal Credit { get; set; }
        public string ClosingNote { get; set; }
    }
}
