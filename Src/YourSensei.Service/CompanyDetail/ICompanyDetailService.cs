using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.Service
{
    public interface ICompanyDetailService
    {
        Task<Data.CompanyDetail> GetProfileByID(string id);

        Task<List<Data.CompanyDetail>> GetCompanies();
    }
}
