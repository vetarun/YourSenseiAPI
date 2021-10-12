using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourSensei.Data;
using YourSensei.ViewModel;

namespace YourSensei.Service
{
    public interface IMentorService
    {
        Task<List<MentorResponseViewModel>> GetAllMentorsByCompanyID(string companyID);
        Task<ResponseViewModel> AddMentor(MentorResponseViewModel emp);
        Task<ResponseViewModel> DeleteMentor(string mentorid);
        Task<MentorResponseViewModel> GetMentorById(string mentorid);
        Task<ResponseViewModel> UpdateMentor(MentorResponseViewModel emp);
        Task<List<GetMentorByIsActivedatViewModel>> GetMentorsByIsActive(string companyID, bool isActive);
        Task<bool> IsExists(string mentorId);
        Task<Mentor> GetMentorByEmployeeID(string employeeID);
    }
}
