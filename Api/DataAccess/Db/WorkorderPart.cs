using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class WorkorderPart
{
    public int PartsIssueid { get; set; }

    public int? WorkorderId { get; set; }

    public int? Quantity { get; set; }

    public string? Manufacturer { get; set; }

    public string? Sku { get; set; }

    public string? Description { get; set; }

    public int? Erfid { get; set; }

    public string? ProdNo { get; set; }

    public string? ModelNo { get; set; }

    public decimal? StandardCost { get; set; }

    public decimal? Tpspcost { get; set; }

    public decimal? Total { get; set; }

    public int? AssetId { get; set; }

    public bool? NonSerializedIssue { get; set; }

    public bool? PartReplenish { get; set; }

    public virtual WorkOrder? Workorder { get; set; }
}
