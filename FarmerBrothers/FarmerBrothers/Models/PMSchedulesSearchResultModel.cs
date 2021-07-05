using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FarmerBrothers.Models
{
    public class PMSchedulesSearchResultModel
    {
        public PMSchedulesSearchResultModel() { }
        public int ID { get; set; }
        public string FBNo { get; set; }
        public string ContactID { get; set; }
        public string CustomerName { get; set; }
        public string TechID { get; set; }
        public string TechName { get; set; }
        public string ContactName { get; set; }
        public string Phone { get; set; }
        public string StartDate { get; set; }
        public string IntervalType { get; set; }
        public string NextRunDate { get; set; }
        public string IntervalDuration { get; set; }
        public string Notes { get; set; }
        public string Category { get; set; }
        public bool IsActive { get; set; }
    }
}
