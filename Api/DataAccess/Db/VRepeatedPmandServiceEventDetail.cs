using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class VRepeatedPmandServiceEventDetail
{
    public int WorkorderId { get; set; }

    public int? CustomerId { get; set; }

    public DateTime? WorkorderEntryDate { get; set; }

    public string? SerialNumber { get; set; }

    public int RepeatedEventId { get; set; }

    public DateTime? RepeatedEntryDate { get; set; }

    public string? RepeatSerialNumber { get; set; }

    public DateTime? WorkorderCloseDate { get; set; }

    public DateTime? RepeatedCloseDate { get; set; }

    public string? Category { get; set; }

    public string? RepeatedCategory { get; set; }

    public int? CallTypeid { get; set; }

    public int? RepeatedCallTypeId { get; set; }

    public int? Symptomid { get; set; }

    public string? Expr1 { get; set; }

    public string? CompanyName { get; set; }

    public string? Branch { get; set; }

    public string? CustomerBranch { get; set; }

    public string? RegionNumber { get; set; }

    public string? CustomerRegion { get; set; }

    public int OriginalEventTechId { get; set; }

    public string? OriginalEventTechName { get; set; }

    public string? FamilyAff { get; set; }

    public string? SearchType { get; set; }

    public string? SearchDesc { get; set; }

    public string? Fsmname { get; set; }

    public string? Manufacturer { get; set; }
}
