using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel
{
    public class CardDetailsInputViewModel
    {
        public int ID { get; set; }
        public string CompanyID { get; set; }
        public string UserDetailID { get; set; }
        public string CardNumber { get; set; }
        public string ValidThru { get; set; }
        public string NameOnCard { get; set; }       
        public int CardType { get; set; }
        public bool IsActive { get; set; }
        
    }
}
