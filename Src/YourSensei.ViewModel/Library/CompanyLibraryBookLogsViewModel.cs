using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel
{
    public class CompanyLibraryBookLogsViewModel
    {
        public int CompanyLibraryBookLogID { get; set; }
        public Guid CompanyLibraryBookID { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public string Year { get; set; }
        public string Action { get; set; }
        public string Title { get; set; }
    }
}
