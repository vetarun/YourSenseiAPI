using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel
{
    public class RoleResponseViewModel
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; }
        public Boolean IsActive { get; set; }
    }
}
