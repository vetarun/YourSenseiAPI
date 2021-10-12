using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel
{
   public class PendingApprovalViewModel
    {
        public Guid UserDetailID { get; set; }       
        public Boolean IsApproved { get; set; }
        public string CallType { get; set; }
        public Guid? CompanyID { get; set; }
        public Guid? LoggedInUserId { get; set; }
    }
}
