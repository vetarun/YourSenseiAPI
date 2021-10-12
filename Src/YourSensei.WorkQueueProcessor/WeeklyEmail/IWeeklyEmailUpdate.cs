using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourSensei.ViewModel;
using YourSensei.ViewModel.Dashboard;

namespace YourSensei.WorkQueueProcessor
{
    public interface IWeeklyEmailUpdate
    {
        List<EmployeeResponseViewModel> GetAllEmployee();
        string SendWeeklyEmail(GetDashboardBeltRuleViewModel Pyramid, List<CreditStandingViewModel> credittable, EmployeeResponseViewModel emp);
    }
}
