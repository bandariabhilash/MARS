using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FarmerBrothers.Models
{
    public class ClosurePartsSearchResultModel
    {
        public string FBNo { get; set; }
        public string JDENo { get; set; }
        public string FBStatus { get; set; }
        public string EntryDate { get; set; }
        public string CloseDate { get; set; }
        public string CustomerType { get; set; }
        public string ServiceCenterID { get; set; }
        public string ServiceCompany { get; set; }
        public string FamilyAff { get; set; }
        public string CallTypeID { get; set; }
        public string SolutionID { get; set; }
        public string EntryNo { get; set; }

        public string ItemNo { get; set; }
        public string VendorNo { get; set; }
        public string Description { get; set; }
        public string Supplier { get; set; }
        public string OrderSource { get; set; }
        public string Quantity { get; set; }
        public string Route { get; set; }
        public string Branch { get; set; }
    }
}
