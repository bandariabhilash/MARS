using FarmerBrothers.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class SuperInvoiceModel
    {
            public DateTime? SuperCallFromDate { get; set; }
            public DateTime? SuperCallToDate { get; set; }

            public int DealerId { get; set; }
            public string TechID { get; set; }
            public string ParentACC { get; set; }
            public DateTime? HiddenFromDate { get; set; }
            public DateTime? HiddenToDate { get; set; }
            public int HiddenDealerId { get; set; }
            public string HiddenTechID { get; set; }
            public string HiddenParentACC { get; set; }
        public List<TECH_HIERARCHY> Technicianlist { get; set; }
            public List<Technician> FamilyAffs { get; set; }

            public IList<SuperInvoiceSearchResultModel> SearchResults;
    }

    public class SuperInvoiceSearchResultModel
    {
        public string Region { get; set; }
        public string ESM { get; set; }
        public string Technician { get; set; }
        public string BranchName { get; set; }
        public string FamilyAff { get; set; }
        public string CompanyName { get; set; }
        public string EventID { get; set; }
        public string ContactID { get; set; }
        public string FulfillmentStatus { get; set; }
        public string EntryDate { get; set; }
        public string CloseDate { get; set; }
        public string StartDateTime { get; set; }
        public string ArrivalDateTime { get; set; }
        public string CompletionDateTime { get; set; }
        public string PricingParentID { get; set; }
        public string TechID { get; set; }
        public string BranchNumber { get; set; }
        public string InvoiceNo { get; set; }
        public string Address1 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string OldSearchType { get; set; }
        public string CallTypeID { get; set; }
        public string CallTypeDesc { get; set; }
        public string Scheduledate { get; set; }
        public string CustomerBranchNo { get; set; }
        public string RegionNumber { get; set; }
        public string CustomerBranch { get; set; }
        public string SearchType { get; set; }
        public string SearchDesc { get; set; }
        public string Route { get; set; }
        public string PricingParentDesc { get; set; }
        public string CustomerRegion { get; set; }
        public string DoNotPay { get; set; }
        public string DoNotPayComments { get; set; }


        public string TechId { get; set; }
        public string ToatlEventsByTech { get; set; }
        public string AvgMinsOnsitePerCall { get; set; }
        public string AvgMinsPerCall { get; set; }
        public string ElapsedTimeOnSite { get; set; }
        public string ElapsedTime { get; set; }
        public int TotalCount { get; set; }




    }
}