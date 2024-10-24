using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class TmpBlackBerryScfinvoiceInfo
{
    public int UniqueInvoiceId { get; set; }

    public int? WorkorderId { get; set; }

    public int? TechId { get; set; }

    public string? InvoiceNo { get; set; }

    public DateTime? StartDateTime { get; set; }

    public DateTime? ArrivalDateTime { get; set; }

    public DateTime? CompletionDateTime { get; set; }

    public DateTime? EntryDate { get; set; }

    public int? InvoiceKey { get; set; }

    public string? TechName { get; set; }

    public string? SignedBy { get; set; }

    public string? Signature { get; set; }

    public bool? WaterTested { get; set; }

    public string? HardnessRating { get; set; }

    public string? CustomerSign { get; set; }

    public string? TechnicianSign { get; set; }

    public string? StateofEquipment { get; set; }

    public string? ServiceDelayReason { get; set; }

    public string? TroubleshootSteps { get; set; }

    public string? FollowupComments { get; set; }

    public string? ReviewedBy { get; set; }

    public string? IsUnderWarrenty { get; set; }

    public string? WarrentyFor { get; set; }

    public string? AdditionalFollowupReq { get; set; }

    public string? IsOperational { get; set; }
}
