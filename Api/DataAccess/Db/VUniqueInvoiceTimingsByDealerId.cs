using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class VUniqueInvoiceTimingsByDealerId
{
    public DateTime? StartDateTime { get; set; }

    public DateTime? ArrivalDateTime { get; set; }

    public DateTime? CompletionDateTime { get; set; }

    public string? ResponsibleTechName { get; set; }

    public int? ResponsibleTechid { get; set; }

    public string? BranchNumber { get; set; }

    public string? FamilyAff { get; set; }

    public string? InvoiceNo { get; set; }

    public int? WorkorderId { get; set; }

    public string? BranchName { get; set; }

    public string? Region { get; set; }

    public string? Esm { get; set; }

    public string? RegionNumber { get; set; }
}
