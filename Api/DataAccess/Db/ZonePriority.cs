using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class ZonePriority
{
    public int ZoneIndex { get; set; }

    public string? ZoneName { get; set; }

    public int? ResponsibletechId { get; set; }

    public string? ResponsibleTechName { get; set; }

    public string? ResponsibleTechPhone { get; set; }

    public int? TechTeamLead { get; set; }

    public int? ResponsibleTechBranch { get; set; }

    public int? SecondaryTechId { get; set; }

    public string? SecondaryTechName { get; set; }

    public string? SecondaryTechPhone { get; set; }

    public int? SecondaryTechBranch { get; set; }

    public int? Fsm { get; set; }

    public int? Rsm { get; set; }

    public int? Tsm { get; set; }

    public string? TechTeamLeadName { get; set; }

    public string? ResponsibleTechBranchName { get; set; }

    public string? SecondaryTechBranchName { get; set; }

    public string? Rsmname { get; set; }

    public string? Fsmname { get; set; }

    public int? PhoneSolveTechId { get; set; }

    public string? PhoneSolveTechName { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public string? Coordinates { get; set; }

    public int? OnCallGroupId { get; set; }

    public int? OnCallPrimarytechId { get; set; }

    public int? OnCallBackupTechId { get; set; }
}
