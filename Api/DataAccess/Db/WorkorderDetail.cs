using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class WorkorderDetail
{
    public int? WorkorderId { get; set; }

    public int WorkorderDetailid { get; set; }

    public string? InvoiceNo { get; set; }

    public DateTime? InvoiceDate { get; set; }

    public DateTime? StartDateTime { get; set; }

    public DateTime? ArrivalDateTime { get; set; }

    public DateTime? CompletionDateTime { get; set; }

    public int? ResponsibleTechid { get; set; }

    public string? ResponsibleTechName { get; set; }

    public decimal? Mileage { get; set; }

    public string? TravelTime { get; set; }

    public int? PhoneSolveid { get; set; }

    public int? OriginalWorkorder { get; set; }

    public int? SpawnReason { get; set; }

    public string? CustomerName { get; set; }

    public string? CustomerEmail { get; set; }

    public byte[]? CustomerSignature { get; set; }

    public int? EntryUserid { get; set; }

    public DateTime? EntryDate { get; set; }

    public int? ModifiedUserid { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? SpecialClosure { get; set; }

    public int? SolutionId { get; set; }

    public string? CustomerSignatureDetails { get; set; }

    public int? Nsrreason { get; set; }

    public bool? WaterTested { get; set; }

    public string? HardnessRating { get; set; }

    public string? TechnicianSignatureDetails { get; set; }

    public string? CustomerSignatureBy { get; set; }

    public decimal? TotalDissolvedSolids { get; set; }

    public string? StateofEquipment { get; set; }

    public string? ServiceDelayReason { get; set; }

    public string? TroubleshootSteps { get; set; }

    public string? FollowupComments { get; set; }

    public string? OperationalComments { get; set; }

    public string? ReviewedBy { get; set; }

    public string? IsUnderWarrenty { get; set; }

    public string? WarrentyFor { get; set; }

    public string? AdditionalFollowupReq { get; set; }

    public string? IsOperational { get; set; }

    public virtual WorkOrder? Workorder { get; set; }
}
