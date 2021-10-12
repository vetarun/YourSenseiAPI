using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel
{
    public class SelectedCompanyLibraryBookLogsViewModel
    {
        public List<int> SelectedIds { get; set; }
        public string CompanyID { get; set; }
        public string UserDetailID { get; set; }
    }
}
