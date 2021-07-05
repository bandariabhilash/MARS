using FarmerBrothers.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class ScheduleEventModel
    {
        public int WorkorderID { get; set; }
        public int TechID { get; set; }
        public DateTime ScheduleDate { get; set; }
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string Notes { get; set; }

        public IList<AllFBStatu> RescheduleReasonCodesList;
        public Nullable<int> ReasonCode { get; set; }
    }
}