using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.Utility
{
    public enum UserCategory
    {
        [DescriptionAttribute("Super Admin")]
        SuperAdmin,
        [DescriptionAttribute("Company Admin")]
        CompanyAdmin,
        [DescriptionAttribute("Individual")]
        Individual,
        [DescriptionAttribute("Company User")]
        CompanyUser,
        [DescriptionAttribute("Company External Mentor")]
        CompanyExternalMentor,
        [DescriptionAttribute("Global Mentor")]
        GlobalMentor
    }
}
