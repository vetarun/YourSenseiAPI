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
    
    public partial class TrainingEventAttendee
    {
        public System.Guid ID { get; set; }
        public System.Guid TrainigEventID { get; set; }
        public System.Guid EmployeeID { get; set; }
        public Nullable<decimal> Time { get; set; }
        public Nullable<decimal> Test { get; set; }
    }
}
