using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel
{
    public class GetUserDetailResponseViewModel
    {
        public string email { get; set; }
        public string companyId { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string company { get; set; }
        public decimal lastcreditearn { get; set; }
        public decimal totalcredit { get; set; }
        public int totala3 { get; set; }
        public int totalkaizen { get; set; }
        public string beltname { get; set; }
    }
}
