using FarmerBrothers.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class RepeatCallReportSummaryModel
    {
        public DateTime? RepeatCallFromDate { get; set; }
        public DateTime? RepeatCallToDate { get; set; }

        public int DealerId { get; set; }
        public string TechID { get; set; }
        public List<TECH_HIERARCHY> Technicianlist { get; set; }
        public List<Technician> FamilyAffs { get; set; }
        public IList<RepeatCallrptsummaryResultModel> SearchResults { get; set; }

    }
}