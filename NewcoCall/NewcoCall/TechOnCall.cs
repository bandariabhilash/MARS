//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NewcoCall
{
    using System;
    using System.Collections.Generic;
    
    public partial class TechOnCall
    {
        public int TechOnCallID { get; set; }
        public Nullable<int> TechId { get; set; }
        public Nullable<System.DateTime> ScheduleDate { get; set; }
        public Nullable<decimal> ScheduleStartTime { get; set; }
        public Nullable<decimal> ScheduleEndTime { get; set; }
        public Nullable<int> EntryUserID { get; set; }
        public string EntryUserName { get; set; }
        public Nullable<int> ModifiedUserID { get; set; }
        public string ModifiedUserName { get; set; }
        public Nullable<System.DateTime> ScheduleEndDate { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<System.DateTime> ScheduleCreatedDate { get; set; }
    }
}