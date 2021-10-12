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
    public class WorkQueueService: IWorkQueueService
    {
        private readonly YourSensei_DBEntities _context;

        public WorkQueueService(YourSensei_DBEntities context)
        {
            _context = context;
        }

        public async Task<List<WorkQueue>> GetWorkQueueByStatus(string status)
        {
            try
            {
                return await _context.WorkQueues.Where(a => a.Status == status).ToListAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ResponseViewModel> Save(WorkQueue workQueue)
        {
            try
            {
                if (workQueue != null)
                {
                    if (workQueue.ID == 0)
                        _context.Entry(workQueue).State = EntityState.Added;
                    else
                        _context.Entry(workQueue).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

                    return new ResponseViewModel { Code = 200, Message = "Successfully saved" };
                }

                return new ResponseViewModel { Code = 400, Message = "Not found" };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
