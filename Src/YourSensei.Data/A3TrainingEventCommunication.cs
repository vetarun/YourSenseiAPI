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
    
    public partial class A3TrainingEventCommunication
    {
        public int ID { get; set; }
        public System.Guid TrainingEventID { get; set; }
        public System.Guid SentBy { get; set; }
        public System.DateTime SentDate { get; set; }
        public string Message { get; set; }
        public string MessageSender { get; set; }
    }
}
