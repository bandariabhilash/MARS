using FarmerBrothers.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class Charts
    {
        public DateTime? RedirectCallFromDate { get; set; }
        public DateTime? RedirectCallToDate { get; set; }

        public int DealerId { get; set; }
        public string TechID { get; set; }
        public List<TECH_HIERARCHY> Technicianlist { get; set; }
        public List<Technician> FamilyAffs { get; set; }
       
        public IList<ChartData> SearchResults;
    }

    public class ChartData
    {
        public string xValue;
        public double yValue;
        public string text;
    }
}