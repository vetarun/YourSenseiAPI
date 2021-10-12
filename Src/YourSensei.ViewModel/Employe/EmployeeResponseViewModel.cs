using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel
{
    public class EmployeeResponseViewModel
    {
        public string Id { get; set; }
        public string userDetialID { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string RoleId { get; set; }
        public string MentorId { get; set; }
        public string CompanyId { get; set; }
        public Boolean IsMentor { get; set; }
        public string EmployeeCode { get; set; }
        public decimal Credit { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public Boolean IsActive { get; set; }
        public string UserCategory { get; set; }
        public string OtherRole { get; set; }
        public string MemberID { get; set; }
    }
}
