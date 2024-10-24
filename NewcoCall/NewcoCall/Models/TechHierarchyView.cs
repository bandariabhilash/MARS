using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewcoCall.Models
{
    public class TechHierarchyView
    {

        public string PreferredProvider { get; set; }
        public string ProviderPhone { get; set; }
        public string AreaCode { get; set; }
        public int? RSMId { get; set; }
        public string RSMName { get; set; }
        public string RSMphone { get; set; }
        public int? ESMId { get; set; }
        public string ESMName { get; set; }
        public string ESMphone { get; set; }
        public int? DSMId { get; set; }
        public string DSMName { get; set; }
        public string DSMPhone { get; set; }

        public string Branch { get; set; }
        public string RegionName { get; set; }
        public string PricingParentName { get; set; }
        public string PricingParent { get; set; }
        public string DistributorName { get; set; }
        public string ServiceTire { get; set; }
        public string Route { get; set; }
        public int? TechID { get; set; }
        public string TechType { get; set; }
        public string SearchType { get; set; }
        public string TechTypeDesc { get; set; }
        public string TechZip { get; set; }
        public string TechEmail { get; set; }
        public int CustomerZIP { get; set; }
        public string BranchNumber { get; set; }
        public string BranchName { get; set; }

    }
}