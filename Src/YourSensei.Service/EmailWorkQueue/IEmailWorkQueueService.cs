using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourSensei.Data;
using YourSensei.ViewModel;

namespace YourSensei.Service
{
    public interface IEmailWorkQueueService
    {
        Task<List<EmailWorkQueue>> GetEmailWorkQueueByStatus(string status);

        Task<ResponseViewModel> Save(EmailWorkQueue emailWorkQueue);
    }
}
