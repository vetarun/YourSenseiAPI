using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourSensei.Data;
using YourSensei.ViewModel.Dashboard;

namespace YourSensei.ViewModel
{
    public class GetDashboardBeltRuleViewModel
    {
        public List<PyramidViewModel> ListOfBelt { get; set; }
        public int indexofyouarehere { get; set; }
        public decimal creditwant { get; set; }
        public decimal a3want { get; set; }
        public decimal kaizenwant { get; set; }
        public decimal totalcredit { get; set; }
        //public int ID { get; set; }        
        //public string BeltName { get; set; }
        //public string BeltColor { get; set; }
        //public decimal TotalCredit { get; set; }
        //public int TotalA3 { get; set; }
        //public int TotalKaizen { get; set; }
        //public int OrderValue { get; set; }
        //public bool YouAreHere { get; set; }
    }
}
