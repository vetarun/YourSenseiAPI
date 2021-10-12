using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.Utility
{
    public enum FeaturesAllowed
    {
        [DescriptionAttribute("Book Library")]
        booklibrary,
        [DescriptionAttribute("Dashboard")]
        dashboard,
        [DescriptionAttribute("External Mentor")]
        externalmentor,
        [DescriptionAttribute("Company Setting")]
        companysetting,
        [DescriptionAttribute("Company Profile")]
        companyprofile,
        [DescriptionAttribute("Employee")]
        employee,
        [DescriptionAttribute("Training Event")]
        trainingevent,
        [DescriptionAttribute("Credit Log")]
        creditlog,
        [DescriptionAttribute("Quiz")]
        quiz,
        [DescriptionAttribute("Tool")]
        tool,
    }
}
