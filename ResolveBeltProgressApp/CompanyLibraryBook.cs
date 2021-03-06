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
    
    public partial class CompanyLibraryBook
    {
        public System.Guid ID { get; set; }
        public Nullable<System.Guid> CompanyID { get; set; }
        public Nullable<System.Guid> UserDetailID { get; set; }
        public string CoverImageUrl { get; set; }
        public string CoverImageOriginalFileName { get; set; }
        public string Author { get; set; }
        public string Title { get; set; }
        public string Publisher { get; set; }
        public string Year { get; set; }
        public decimal Rating { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public System.Guid CreatedBy { get; set; }
        public System.Guid ModifiedBy { get; set; }
        public bool IsActive { get; set; }
        public System.Guid BookCategoryID { get; set; }
        public string SubTitle { get; set; }
        public string BookUrl { get; set; }
        public string BookOriginalFileName { get; set; }
        public decimal Credit { get; set; }
        public Nullable<bool> IsAccepted { get; set; }
        public Nullable<System.Guid> ParentBookID { get; set; }
        public int QuizID { get; set; }
        public bool IsModified { get; set; }
        public bool IsVersion { get; set; }
    }
}
