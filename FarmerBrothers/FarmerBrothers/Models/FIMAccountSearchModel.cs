using FarmerBrothers.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FarmerBrothers.Models
{
    public enum FIMAccountSearchSubmitType
    {
        SEARCH = 0,
    }

    public class FIMAccountSearchModel
    {
        public string VendorID { get; set; }
        public string VendorName { get; set; }
        public string JMSLogin { get; set; }
        public IList<FIMAccountSearchModelResult> SearchResults;
    }
}