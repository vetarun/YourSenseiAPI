//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace YourSensei.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class UserDetail
    {
        public System.Guid ID { get; set; }
        public System.Guid EmployeeID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.Guid CreatedBy { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public System.Guid ModifiedBy { get; set; }
        public bool IsActive { get; set; }
        public Nullable<System.Guid> UserType { get; set; }
        public Nullable<System.DateTime> RequestDate { get; set; }
        public bool IsApproved { get; set; }
        public Nullable<System.DateTime> ApprovalDate { get; set; }
        public bool IsRejected { get; set; }
        public Nullable<System.DateTime> RejectedDate { get; set; }
        public bool IsInitialPassword { get; set; }
    }
}
