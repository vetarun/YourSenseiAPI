using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel
{
    public class CreditLogByCompanyIdResponseViewModel 
    {
        public string EmpCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public decimal SumOfLogs { get; set; }
        public List<CreditLogResponseViewModel> ListOfLogsForuser { get; set; }

    }
}
