using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel
{
    public class BeltRuleInputViewModel
    {
        public int ID { get; set; }
        public string CompanyID { get; set; }
        public string UserDetailID { get; set; }
        public string BeltName { get; set; }
        public string BeltColor { get; set; }
        public decimal TotalCredit { get; set; }
        public int TotalA3 { get; set; }
        public int TotalKaizen { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public bool IsActive { get; set; }
        public bool isIndividual { get; set; }
    }
}
