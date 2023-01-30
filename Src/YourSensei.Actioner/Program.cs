using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using Unity.Lifetime;
using YourSensei.WorkQueueProcessor;

namespace YourSensei.Actioner
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new UnityContainer();
            container.RegisterType<IWorkQueueProcessor, WorkQueueProcessor.WorkQueueProcessor>(new HierarchicalLifetimeManager());
            WorkQueueProcessor.WorkQueueProcessor workQueueProcessor = container.Resolve<WorkQueueProcessor.WorkQueueProcessor>();
            Console.WriteLine("I am here");
            if (args[0] == "ProcessDailyAt12AM")
            {
                workQueueProcessor.ProcessDailyAt12AM();
            }
            else if (args[0] == "ProcessWeeklyOnMondayAt12")
            {
                workQueueProcessor.WeeklyEmailUpdate();
            }
            else if (args[0] == "ProcessEmailEvery5Minutes")
            {
                workQueueProcessor.ProcessEmailEvery5Minutes();
        }

    }
    }
}
