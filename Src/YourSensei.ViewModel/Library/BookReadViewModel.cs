using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel
{
    public class BookReadViewModel
    {
        public string ID { get; set; }
        public string CompanyID { get; set; }
        public string EmployeeID { get; set; }
        public string BookID { get; set; }
        public int Rating { get; set; }
        public string Comments { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Title { get; set; }
        public DateTime Completeddate { get; set; }
        public string CompanyName { get; set; }
    }
}
