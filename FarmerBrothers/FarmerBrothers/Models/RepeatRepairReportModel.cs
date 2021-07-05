using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class RepeatRepairlReportModel
    {
        public string EventID { get; set; }
        public string CustomerID { get; set; }
        public string CustomerType { get; set; }
        public string CustomerName { get; set; }
        public string CustomerBranchID { get; set; }
        public string BranchName { get; set; }
        public string Region { get; set; }
        public string ESM { get; set; }
        public string SerialNumber { get; set; }
        public string Manufacturer { get; set; }
        public string WorkorderID { get; set; }
        public string EntryDate { get; set; }
        public string originalWorkorderID { get; set; }
        public string OriginalWrkorderEntryDate { get; set; }
        public string OriginalWrkorderClosedDate { get; set; }
        public string OrgTechName { get; set; }
        public string OrgTechId { get; set; }
        public string FamilyAff { get; set; }
    }

    public class OriginalEventDetailsModel
    {
        public string EventID { get; set; }
        public string Status { get; set; }
        public string EntryDate { get; set; }
        public string CustomerRegion { get; set; }
        public string CustomerRegionName { get; set; }
        public string CustomerBranch { get; set; }
        public string CustomerBranchName { get; set; }
        public string JDE { get; set; }
        public string CompanyName { get; set; }
        public string Address1 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string DispatchCompany { get; set; }
        public string DispatchDate { get; set; }
        public string ElapsedTime { get; set; }
    }

    public class OriginalEventDetailsResultsModel
    {
        public IList<OriginalEventDetailsModel> SearchResults { get; set; }
    }

}