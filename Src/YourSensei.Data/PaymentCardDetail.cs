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
    
    public partial class PaymentCardDetail
    {
        public int ID { get; set; }
        public Nullable<System.Guid> CompanyID { get; set; }
        public Nullable<System.Guid> UserDetailID { get; set; }
        public string CardNumber { get; set; }
        public string ValidThru { get; set; }
        public string NameOnCard { get; set; }
        public bool IsActive { get; set; }
        public System.Guid CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.Guid ModifiedBy { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public int CardType { get; set; }
    }
}
