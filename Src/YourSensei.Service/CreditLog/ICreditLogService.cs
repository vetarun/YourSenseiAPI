using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourSensei.Data;
using YourSensei.ViewModel;

namespace YourSensei.Service
{
    public interface ICreditLogService
    {
        Task<ResponseViewModel> SaveCreditLog(EmployeeResponseViewModel employee, decimal credit, string id, string logType);

        Task<List<CreditLogByCompanyIdResponseViewModel>> GetCreditLogsByCompanyID(string companyID);
        Task<List<CreditLogResponseViewModel>> GetCreditLogsByUserID(string userid, string companyID);
        Task<List<CreditLogResponseViewModel>> GetAllEmployeewithMentor(string employeeid);
        Task<CreditLogByEmployeeIdResponseViewModel> GetCreditLogsByLoggedInUser(string employeeid, bool isActive);
        Task<Boolean> IsMentorLoggedIn(string employeeid);

    }
}
