using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel
{
    public class CreditLogResponseViewModel 
    {
       
        public string MenberID { get; set; }
        public Guid EmpID { get; set; }
        public string EmpCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public string Description { get; set; }
        public DateTime AwardedDate { get; set; }
        public decimal Credit { get; set; }
        public string Event { get; set; }
        public Guid UserID { get; set; }
       
    }
}
