using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourSensei.ViewModel;


namespace YourSensei.Service
{
   public interface ICompanySettingService
    {
        //Task<List<CompanySettingViewModel>> GetAllCustomSetting();
        Task<ResponseViewModel> AddCompanySetting(CompanySettingViewModel obj);
        // Task<string> GetAllCompanySetting(string companyid);
        Task<CompanySettingViewModel> GetAllCompanySetting(string companyid);
        Task<CompanySettingViewModel> GetCompanyById(string companyid);
    }
}
