using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel.Dashboard
{
    public class CreditStandingViewModel
    {
        
         public string EmployeeName{get;set;}  
         public string MentorName{get;set;}
         public decimal CICredits{get;set;}
         public Nullable<Guid> UserDetailID {get;set;}
         public Nullable<Guid> CompanyID {get;set;}
         public DateTime AwardedDate{get;set;}
         public Guid EmployeeID {get;set;}
         public Nullable<Guid> TrainingEventID{get;set;}
         public decimal DollarImpacted {get;set;}
        public decimal? CILastWeek { get; set; }
        public string BeltName { get; set; }
        public string BeltColor { get; set; }

    }
}
