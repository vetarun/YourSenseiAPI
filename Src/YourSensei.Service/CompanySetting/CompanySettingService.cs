using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourSensei.Data;
using YourSensei.ViewModel;

namespace YourSensei.Service
{
    public class CompanySettingService : ICompanySettingService
    {
        private readonly YourSensei_DBEntities _context;

        public CompanySettingService(YourSensei_DBEntities context)
        {
            _context = context;
        }

        public async Task<ResponseViewModel> AddCompanySetting(CompanySettingViewModel obj)
        {
            YourSensei.Data.CompanySetting objcmpnyset = new YourSensei.Data.CompanySetting();
            try
            {
                var data = await _context.CompanySettings.Where(a => a.CompanyId == new Guid(obj.CompanyId)).ToListAsync();

                if (data.Count == 0)
                {
                    objcmpnyset.ID = Guid.NewGuid();
                    objcmpnyset.CompanyId = new Guid(obj.CompanyId);
                    objcmpnyset.IsMentorMandatory = obj.IsMentorMandatory;
                    objcmpnyset.GlobalAverageBookRating = obj.GlobalAverageBookRating;
                    objcmpnyset.GlobalMentor = obj.GlobalMentor;
                    objcmpnyset.GlobalBookList = obj.GlobalBookList;
                    objcmpnyset.CreatedBy = new Guid(obj.CreatedBy);
                    objcmpnyset.CreatedDate = DateTime.UtcNow;
                    objcmpnyset.ModifiedBy = new Guid(obj.ModifiedBy);
                    objcmpnyset.ModifiedDate = DateTime.UtcNow;
                    objcmpnyset.IsActive = true;
                    objcmpnyset.A3DollarApprover = new Guid(obj.A3DollarApprover);
                    _context.CompanySettings.Add(objcmpnyset);
                    _context.SaveChanges();

                    return new ResponseViewModel { Code = 200, Message = "Company Setting has been created successfully!" };
                }
                else
                {
                    var existdata = await _context.CompanySettings.Where(a => a.CompanyId == new Guid(obj.CompanyId)).FirstOrDefaultAsync();
                    existdata.IsMentorMandatory = obj.IsMentorMandatory;
                    existdata.GlobalAverageBookRating = obj.GlobalAverageBookRating;
                    existdata.GlobalMentor = obj.GlobalMentor;
                    existdata.GlobalBookList = obj.GlobalBookList;
                    existdata.CreatedBy = new Guid(obj.CreatedBy);
                    existdata.ModifiedBy = new Guid(obj.ModifiedBy);
                    existdata.ModifiedDate = DateTime.UtcNow;
                    existdata.IsActive = true;
                    existdata.A3DollarApprover = new Guid(obj.A3DollarApprover);
                    _context.SaveChanges();

                    return new ResponseViewModel { Code = 200, Message = "Company Setting has been Updated successfully!" };
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<CompanySettingViewModel> GetAllCompanySetting(string companyid)
        {
            try
            {
                var data = await _context.CompanySettings.Where(a => a.CompanyId == new Guid(companyid)).Select(a => new CompanySettingViewModel
                {
                    IsMentorMandatory = a.IsMentorMandatory,
                    GlobalAverageBookRating = a.GlobalAverageBookRating,
                    GlobalBookList = a.GlobalBookList,
                    GlobalMentor = a.GlobalMentor,
                    A3DollarApprover = a.A3DollarApprover.ToString()
                }).FirstOrDefaultAsync();
                return data;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<CompanySettingViewModel> GetCompanyById(string companyid)
        {
            try
            {
                var data = await _context.CompanySettings.Where(a => a.CompanyId == new Guid(companyid)).Select(a => new CompanySettingViewModel
                {
                    IsMentorMandatory = a.IsMentorMandatory,
                    GlobalAverageBookRating = a.GlobalAverageBookRating,
                    GlobalBookList = a.GlobalBookList,
                    GlobalMentor = a.GlobalMentor,
                    A3DollarApprover = a.A3DollarApprover.ToString()
                }).FirstOrDefaultAsync();
                return data;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}
