using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FarmerBrothers.Data;

namespace FarmerBrothers.Models
{

    public class ThirdPartyMaintenanceModel
    {
        public List<ThirdParty> ThirdPartyList { get; set; }
        public List<string> ThirdPartyBasedOnList = new List<string> { "Per Hour", "Flat Rate", "Per Mile" };
        public List<Data.ThirdpartyConMaintenanceZonerate> ThirdPartyZoneRatesList { get; set; }
        public int ContractMaintenanceid { get; set; }
        public int Techid { get; set; }
        public Nullable<decimal> PartsUpCharge { get; set; }
        public Nullable<decimal> LaborHourlyRate { get; set; }
        public Nullable<decimal> LaborOvertimeRate { get; set; }
        public bool MinOneHourFlag { get; set; }
        public bool RatePerPalletFlag { get; set; }
        public Nullable<decimal> RatePerPallet { get; set; }
        public bool TravelRatePerMileFlag { get; set; }
        public Nullable<decimal> TravelRatePerMile { get; set; }
        public bool TravelAllowRoundTripFlag { get; set; }
        public bool TravelMinOneHour { get; set; }
        public bool TravelHourlyRateFlag { get; set; }
        public Nullable<decimal> TravelHourlyRate { get; set; }
        public bool TravelOverTimeRateFlag { get; set; }
        public Nullable<decimal> TravelOvertimeRate { get; set; }
        public bool TravelZoneRateFlag { get; set; }
        public string thirdpartyzonerates { get; set; }

    }

    public class ThirdParty
    {
        public string ThirdPartyName { get; set; }
        public int ThirdPartyId { get; set; }

    }

  
}