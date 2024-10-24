using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class ThirdPartyContractMaintenance
{
    public int ContractMaintenanceid { get; set; }

    public int Techid { get; set; }

    public decimal? PartsUpCharge { get; set; }

    public decimal? LaborHourlyRate { get; set; }

    public decimal? LaborOvertimeRate { get; set; }

    public bool? MinOneHourFlag { get; set; }

    public bool? RatePerPalletFlag { get; set; }

    public decimal? RatePerPallet { get; set; }

    public bool? TravelRatePerMileFlag { get; set; }

    public decimal? TravelRatePerMile { get; set; }

    public bool? TravelAllowRoundTripFlag { get; set; }

    public bool? TravelMinOneHour { get; set; }

    public bool? TravelHourlyRateFlag { get; set; }

    public decimal? TravelHourlyRate { get; set; }

    public bool? TravelOverTimeRateFlag { get; set; }

    public decimal? TravelOvertimeRate { get; set; }

    public bool? TravelZoneRateFlag { get; set; }
}
