using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class FBDispatchAcceptedModel
    {    
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public IList<FBDispatchAcceptedSearchResults> SearchResults { get; set; }
    }
    public class FBDispatchAcceptedSearchResults
    {
        public string TDate { get; set; }
        public string UserID { get; set; }
        public string WorkorderID { get; set; }
        public string UserName { get; set; }
        public string statusFrom { get; set; }
        public string statusTo { get; set; }
        public string AcceptedDate { get; set; }
    }
}