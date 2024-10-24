using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class Yherfmaster
{
    public int? ContactId { get; set; }

    public int? ChannelId { get; set; }

    public string? SalesPerson { get; set; }

    public int? EntryUserId { get; set; }

    public DateTime? EntryDate { get; set; }

    public int? ModifiedUserId { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int? ReasonId { get; set; }

    public DateTime? DateOnErf { get; set; }

    public DateTime? DateErfreceived { get; set; }

    public DateTime? DateErfprocessed { get; set; }

    public DateTime? TimeErfprocessed { get; set; }

    public DateTime? OriginalRequestedDate { get; set; }

    public string? ContactName { get; set; }

    public string? ContactPhone { get; set; }

    public string? HoursOfOperation { get; set; }

    public string? InstallLocation { get; set; }

    public int? EventId { get; set; }

    public string? Approved { get; set; }

    public int? WeeklyCoffeeVol { get; set; }

    public int? WeeklyTeaVol { get; set; }

    public int? WeeklyLiquidVol { get; set; }

    public int? WeeklyCappuccinoVol { get; set; }

    public string? Erfnotes { get; set; }

    public int? Timezone { get; set; }

    public string? DayLightSaving { get; set; }

    public int? FiscalYear { get; set; }

    public int? FiscalPeriod { get; set; }

    public int? FiscalWeek { get; set; }

    public int? FiscalId { get; set; }

    public DateTime? EquipEtadate { get; set; }

    public DateTime? EquipOrderdate { get; set; }

    public int? ShipToJde { get; set; }

    public string? SiteReady { get; set; }

    public int? BilltoJde { get; set; }

    public DateTime? JdeprocessDate { get; set; }

    public string? ShipToName { get; set; }

    public string? Erfno { get; set; }
}
