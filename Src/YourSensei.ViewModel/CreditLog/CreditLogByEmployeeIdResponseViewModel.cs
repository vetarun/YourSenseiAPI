using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourSensei.Data;

namespace YourSensei.ViewModel
{
    public class CreditLogByEmployeeIdResponseViewModel
    {
        public List<usp_GetCreditLogsByUserDetailID_Result> ListOflogs { get; set; }
        public decimal SumOfCredits { get; set; }
    }
}
