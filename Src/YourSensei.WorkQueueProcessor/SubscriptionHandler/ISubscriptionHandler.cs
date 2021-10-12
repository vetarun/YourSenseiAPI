using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourSensei.Data;

namespace YourSensei.WorkQueueProcessor
{
    public interface ISubscriptionHandler
    {
        string ProcessRenewRequest(WorkQueue workQueue);
        void ProcessingExpirydate();
    }
}
