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
    
    public partial class InitialAssessmentQuestion
    {
        public int ID { get; set; }
        public int InitialAssessmentCategoryID { get; set; }
        public string QuestionName { get; set; }
        public bool IsActive { get; set; }
        public System.Guid CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.Guid ModifiedBy { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    }
}
