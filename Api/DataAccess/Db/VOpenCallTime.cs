using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class VOpenCallTime
{
    public int WorkorderId { get; set; }

    public DateTime? WorkorderEntryDate { get; set; }

    public int? Techid { get; set; }

    public string? DispatchCompany { get; set; }

    public DateTime? ScheduleDate { get; set; }

    public int? ScheduleUserid { get; set; }

    public string? ElapsedTime { get; set; }

    public int ElapsedTimeInMins { get; set; }

    public string? WorkorderCallstatus { get; set; }

    public string? CompanyName { get; set; }

    public string? Address1 { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? PostalCode { get; set; }

    public int ContactId { get; set; }

    public string? CustomerRegion { get; set; }

    public string? CustomerBranchNo { get; set; }

    public string? RegionNumber { get; set; }

    public string? CustomerBranch { get; set; }

    public string? FamilyAff { get; set; }

    public string? SearchType { get; set; }

    public string? SearchDesc { get; set; }

    public string? AssignedStatus { get; set; }
}
