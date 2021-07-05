using FarmerBrothers.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class RedirectCallModel
    {
        public DateTime? RedirectCallFromDate { get; set; }
        public DateTime? RedirectCallToDate { get; set; }

        public int DealerId { get; set; }
        public string TechID { get; set; }
        public List<TECH_HIERARCHY> Technicianlist { get; set; }
        public List<Technician> FamilyAffs { get; set; }

        public IList<RedirectCallSearchResultModel> SearchResults;
    }

    public class RedirectCallSearchResultModel
    {
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string TechName { get; set; }
        public string TechId { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string RedirectCount { get; set; }
    }
}