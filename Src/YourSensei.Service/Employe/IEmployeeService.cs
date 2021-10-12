using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourSensei.Data;
using YourSensei.ViewModel;

namespace YourSensei.Service
{
    public interface IEmployeeService
    {
        Task<List<EmployeeResponseViewModel>> GetAllEmployee(string companyid);

        Task<ResponseViewModel> AddEmployee(EmployeeResponseViewModel emp);

        Task<List<RoleResponseViewModel>> GetAllRole();

        Task<List<EmployeeResponseViewModel>> GetAllMentor(string companyid);

        Task<ResponseViewModel> DeleteEmployee(string employeeid);

        Task<EmployeeResponseViewModel> GetEmployeeById(string empid);

        Task<ResponseViewModel> UpdateEmployee(EmployeeResponseViewModel emp);

        Task<Employee> GetProfileByEmail(string email);

        Task<Mentor> GetMentorProfileByEmail(string email);

        Task<List<GetEmployeeByMentorViewModel>> GetEmployeeByMentorID(string mentorID);

        Task<Employee> GetEmployeeByUserDetailID(Guid userDetailID);
    }
}
