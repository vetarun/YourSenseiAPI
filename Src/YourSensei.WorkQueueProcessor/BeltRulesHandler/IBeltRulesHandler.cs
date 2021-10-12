using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourSensei.Data;

namespace YourSensei.WorkQueueProcessor.BeltAchievementHandler
{
    public interface IBeltRulesHandler
    {

        void ProcessBeltAchievement();
        string deleteBeltRules(WorkQueue workQueue);
    }
}
