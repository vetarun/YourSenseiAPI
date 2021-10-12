using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel
{
  public  class CompanySettingViewModel
    {
        public int Id { get; set; }
        public string CompanyId { get; set; }        
        public Boolean IsMentorMandatory { get; set; }
        public Boolean GlobalAverageBookRating { get; set; }
        public Boolean GlobalMentor { get; set; }
        public Boolean GlobalBookList { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public Boolean IsActive { get; set; }
        public string A3DollarApprover { get; set; }

    }
}
