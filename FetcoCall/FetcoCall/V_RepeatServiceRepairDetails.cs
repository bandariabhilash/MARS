//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FetcoCall
{
    using System;
    using System.Collections.Generic;
    
    public partial class V_RepeatServiceRepairDetails
    {
        public int WorkorderID { get; set; }
        public Nullable<int> CustomerID { get; set; }
        public Nullable<System.DateTime> WorkorderEntryDate { get; set; }
        public string SerialNumber { get; set; }
        public int RepeatedEventId { get; set; }
        public Nullable<System.DateTime> RepeatedEntryDate { get; set; }
        public string RepeatSerialNumber { get; set; }
        public Nullable<System.DateTime> WorkorderCloseDate { get; set; }
        public Nullable<System.DateTime> RepeatedCloseDate { get; set; }
        public string Category { get; set; }
        public string RepeatedCategory { get; set; }
        public Nullable<int> CallTypeid { get; set; }
        public Nullable<int> RepeatedCallTypeID { get; set; }
        public Nullable<int> Symptomid { get; set; }
        public string Expr1 { get; set; }
        public string CompanyName { get; set; }
        public string Branch { get; set; }
        public string CustomerBranch { get; set; }
        public string RegionNumber { get; set; }
        public string CustomerRegion { get; set; }
        public int OriginalEventTechID { get; set; }
        public string OriginalEventTechName { get; set; }
        public string FamilyAff { get; set; }
        public string SearchType { get; set; }
        public string SearchDesc { get; set; }
        public string FSMName { get; set; }
        public string Manufacturer { get; set; }
    }
}