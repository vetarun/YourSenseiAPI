//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ResolveBeltProgressApp
{
    using System;
    using System.Collections.Generic;
    
    public partial class BeltRuleLogTransaction
    {
        public int ID { get; set; }
        public Nullable<System.Guid> CompanyID { get; set; }
        public Nullable<System.Guid> UserDetailID { get; set; }
        public int BeltRuleLogID { get; set; }
        public bool IsActive { get; set; }
        public Nullable<System.Guid> AcceptedBy { get; set; }
        public Nullable<System.DateTime> AcceptedDate { get; set; }
    }
}
