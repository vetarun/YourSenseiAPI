using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourSensei.ViewModel
{
    public class LoginResponse : SubscriptionPlanViewModel
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public Guid UserRole { get; set; }
        public string Email { get; set; }
        public Guid CompanyId { get; set; }
        public Guid? Usertypeid { get; set; }
        public string UserTypeName { get; set; }
        public string Token { get; set; }
        public string EmployeeID { get; set; }
        public string MentorID { get; set; }
        public bool IsInternalMentor { get; set; }
        public bool IsInitialPassword { get; set; }
        public string phone { get; set; }
        public bool IsDollarApprover { get; set; }
        public DateTime? SubscriptionExpiryDate { get; set; }
    }
}
