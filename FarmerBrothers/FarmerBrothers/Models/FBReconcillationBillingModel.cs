using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class FBReconcillationBillingModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public IList<FBReconcillationBillingSearchResultModel> SearchResults;
    }
    public class FBReconcillationBillingSearchResultModel
    {
        public string AgentName { get; set; }
        public string ServiceEventCnt { get; set; }
        public string NonServiceEventCnt { get; set; }
        public string SavedEventCnt { get; set; }
        public string SalesEventCnt { get; set; }
        public string ReopenEventCnt { get; set; }
        public string ClosedEventCnt { get; set; }
        public string RefurbEventCnt { get; set; }
        public string RowTot { get; set; }

    }
}