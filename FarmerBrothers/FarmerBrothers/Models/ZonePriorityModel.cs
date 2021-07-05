using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FarmerBrothers.Data;

namespace FarmerBrothers.Models
{
    public class ZonePriorityModel
    {
        public int? ZoneIndex { get; set; }
        public string ZoneName { get; set; }
        public int? ResponsibletechID { get; set; }
        public string ResponsibleTechName { get; set; }
        public int? SecondaryTechID { get; set; }
        public string SecondaryTechName { get; set; }
        public int? Fsm { get; set; }
        public int? OnCallGroupID { get; set; }
        public string OnCallGroup { get; set; }
        public int? OnCallPrimarytechID { get; set; }
        public int? OnCallBackupTechID { get; set; }
        public int? ResponsibleTechBranch { get; set; }
        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public IEnumerable<ZonePriorityModel> SearchResults;

        public IList<Technician> Technicians;

        public IList<OnCallGroup> OnCallGroupList;
    }
}