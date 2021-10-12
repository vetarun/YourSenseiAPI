using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using YourSensei.Data;

namespace YourSensei.Service
{
    public class CompanyDetailService : ICompanyDetailService
    {
        private readonly YourSensei_DBEntities _context;

        public CompanyDetailService(YourSensei_DBEntities context)
        {
            _context = context;
        }

        public async Task<Data.CompanyDetail> GetProfileByID(string id)
        {
            try
            {
                var result = await _context.CompanyDetails.Where(d => d.ID == new Guid(id)).FirstOrDefaultAsync();
                return result;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<Data.CompanyDetail>> GetCompanies()
        {
            try
            {
                var result = await _context.CompanyDetails.ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
