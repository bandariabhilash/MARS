using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class FBDispatchLogModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public IList<FBDispatchSearchResults> SearchResults { get; set; }
    }
    public class FBDispatchSearchResults
    {
        public string TDate { get; set; }
        public string UserID { get; set; }
        public string WorkorderID { get; set; }
        public string UserName { get; set; }
    }
}