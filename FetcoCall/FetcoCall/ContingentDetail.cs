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
    
    public partial class ContingentDetail
    {
        public int ID { get; set; }
        public int ContingentID { get; set; }
        public string Name { get; set; }
        public Nullable<decimal> LaidInCost { get; set; }
        public Nullable<decimal> CashSale { get; set; }
        public Nullable<decimal> Rental { get; set; }
        public Nullable<bool> IsActive { get; set; }
    }
}
