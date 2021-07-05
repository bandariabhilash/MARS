using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class FBDispatchReportModel
    {

        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public IList<FBDispatchReportSearchResults> SearchResults { get; set; }
    }
    public class FBDispatchReportSearchResults
    {
        public string WorkorderID { get; set; }
        public string UserName { get; set; }
        public string TimeElapsed { get; set; }
        public string WorkOrderCallType { get; set; }
        
    }
}