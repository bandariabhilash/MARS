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
    
    public partial class NonSerialized
    {
        public int NSerialid { get; set; }
        public Nullable<int> Erfid { get; set; }
        public Nullable<int> WorkorderID { get; set; }
        public string Model { get; set; }
        public Nullable<int> OrigOrderQuantity { get; set; }
        public Nullable<int> ExpectedQty { get; set; }
        public Nullable<int> ShippedQuantity { get; set; }
        public string ManufNumber { get; set; }
        public string Description { get; set; }
        public string Bin { get; set; }
        public string Catalogid { get; set; }
        public string TagType { get; set; }
        public Nullable<decimal> UnitPrice { get; set; }
        public Nullable<decimal> TotalLineAmount { get; set; }
        public Nullable<int> ShippedFrom { get; set; }
        public Nullable<int> FeastMovementid { get; set; }
        public string Location { get; set; }
    
        public virtual WorkOrder WorkOrder { get; set; }
    }
}
