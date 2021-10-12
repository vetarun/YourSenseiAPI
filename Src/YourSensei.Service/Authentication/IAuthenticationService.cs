using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourSensei.Data;
using YourSensei.ViewModel;

namespace YourSensei.Service
{
    public interface IAuthenticationService
    {
        Task<ResponseViewModel> SignUp(SignupInputViewModel comp);

        Task<LoginResponse> Login(string email, string pass);
        
        Task<ResponseViewModel> ResetPassword(string link, string password, string email, string oldpassword);

        Task<ResponseViewModel> ForgotPassword(string email);
       
        Task<List<usp_GetRegistrationDetails_Result>> AllPendingApproval(bool approved, bool rejected);

        Task<ResponseViewModel> AcceptRejectList(List<PendingApprovalViewModel> lst);

        Task<ResponseViewModel> SendSupport(TechSupportInputViewModel input);

        Task<List<PaymentCardDetail>> GetPaymentCardList(int id, string companyId, string UserId);

        Task<ResponseViewModel> AddUpdateCardDetails(CardDetailsInputViewModel input);
        Task<UserDetail> GetSuperAdminDetail();
        Task<SignupInputViewModel> GetCompanyDetailByUserId(string userid);
        Task<CompanyDetail> GetCompanyDetailByCompanyId(string CompanyId);
        Task<List<GetUserDetailResponseViewModel>> GetUserDetails( string companyId);
    }
}
