using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourSensei.Data;

namespace YourSensei.ViewModel
{
    public class GetEmployeeByMentorViewModel : Employee
    {
        public Guid userId { get; set; }
    }
}
