using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FarmerBrothers.Models
{
    public class TechnicianSearchResultModel
    {
        public string StartDateTime { get; set; }
        public string ArrivalDateTime { get; set; }
        public string CompletionDateTime { get; set; }
        public string ResponsibleTechName { get; set; }
        public string ResponsibleTechid { get; set; }
        public string BranchNumber { get; set; }
        public string FamilyAff { get; set; }
        public string InvoiceNo { get; set; }
        public string WorkorderID { get; set; }
        public string BranchName { get; set; }
        public string Region { get; set; }
        public string Route { get; set; }
        public string ESM { get; set; }

        public string ToatlEventsByTech { get; set; }
        public string ElapsedTime { get; set; }
        public string ElapsedTimeOnSite { get; set; }

    }
}
