using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.Utility
{
    public class EmailHelperInputModel
    {
        public string Subject { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public StreamReader MessageHTML { get; set; }
    }
}
