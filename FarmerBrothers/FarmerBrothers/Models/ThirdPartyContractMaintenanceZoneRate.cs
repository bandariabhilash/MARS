using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class ThirdPartyContractMaintenanceZoneRate
   

       {
        public Nullable<int> ContractMaintenanceid { get; set; }
    public Nullable<int> Techid { get; set; }
    public int? ZoneRateid { get; set; }
    public string Description { get; set; }
    public Nullable<decimal> Rate { get; set; }
    public string BasedOn { get; set; }
}
}