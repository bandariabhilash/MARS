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
    
    public partial class YHSMEvent
    {
        public int UniqueID { get; set; }
        public int EventID { get; set; }
        public int ContactID { get; set; }
        public string FulfillmentStatus { get; set; }
        public Nullable<int> EntryUserID { get; set; }
        public Nullable<System.DateTime> EntryDate { get; set; }
        public Nullable<int> CloseUserID { get; set; }
        public Nullable<System.DateTime> CloseDate { get; set; }
        public Nullable<int> ModifiedUserId { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string ERFNO { get; set; }
        public Nullable<int> EquipCount { get; set; }
        public Nullable<int> CallTypeId { get; set; }
        public string CallTypeDesc { get; set; }
        public Nullable<int> SpawnEvent { get; set; }
        public Nullable<int> TimeZone { get; set; }
        public string DayLightSaving { get; set; }
        public string ClosureConfirmationNo { get; set; }
        public Nullable<int> FSM { get; set; }
        public string SeriviceLevelCode { get; set; }
        public Nullable<System.DateTime> AppointmentDate { get; set; }
        public string recallevent { get; set; }
        public Nullable<int> NewPriorityCode { get; set; }
        public string EventContact { get; set; }
        public Nullable<int> CallPriority { get; set; }
        public string ContactPhone { get; set; }
        public string CallerName { get; set; }
        public Nullable<int> ProjectNumber { get; set; }
    }
}