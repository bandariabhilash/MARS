using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class YhcontactPmuploadsAll
{
    public int UniqueId { get; set; }

    public int? ContactId { get; set; }

    public string? CustomerName { get; set; }

    public string? Description { get; set; }

    public string? EquipmentModel { get; set; }

    public string? EquipmentLocation { get; set; }

    public string? Tpsp { get; set; }

    public int? TechId { get; set; }

    public string? ContactName { get; set; }

    public string? Phone { get; set; }

    public int? TimeZone { get; set; }

    public string? DayLightSaving { get; set; }

    public string? ZipCode { get; set; }

    public DateTime? StartDate { get; set; }

    public string? IntervalType { get; set; }

    public DateTime? NextRunDate { get; set; }

    public short? FirstRun { get; set; }

    public int? IntervalDuration { get; set; }

    public short? EventCreated { get; set; }

    public string? Notes { get; set; }

    public string? TechName { get; set; }

    public short? SpecialUpdate { get; set; }

    public short? SkipDate { get; set; }
}
