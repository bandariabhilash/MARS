using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class FBMonthlyWorkorderModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public IList<FBMonthlyWorkorderSearchResultModel> SearchResults;
    }
    public class FBMonthlyWorkorderSearchResultModel
    {
        public string WorkOrderID { get; set; }
        public string ContactID { get; set; }
        public string CloseDate { get; set; }
        public string UserName { get; set; }
        public string MarsUserCompany { get; set; }
        public string CustCompany { get; set; }
        public string FieldServiceManager { get; set; }
        public string DealerCompanyName { get; set; }
        public string FamilyAff { get; set; }
        public string ClosureConfirmationNo { get; set; }

    }

    public class EscalationReportModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public IList<EscalationReportSearchResultModel> SearchResults;
    }
    public class EscalationReportSearchResultModel
    {
        public string WorkOrderID { get; set; }
        public string WorkorderEntryDate { get; set; }
        public string EscalatedBy { get; set; }
        public string EscalatedTo { get; set; }
        public string EscalatedOn { get; set; }
        public string EventSentTo { get; set; }
        public string TechStatus { get; set; }
    }


}