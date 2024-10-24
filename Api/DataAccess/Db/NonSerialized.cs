using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class NonSerialized
{
    public int Nserialid { get; set; }

    public int? Erfid { get; set; }

    public int? WorkorderId { get; set; }

    public string? Model { get; set; }

    public int? OrigOrderQuantity { get; set; }

    public int? ExpectedQty { get; set; }

    public int? ShippedQuantity { get; set; }

    public string? ManufNumber { get; set; }

    public string? Description { get; set; }

    public string? Bin { get; set; }

    public string? Catalogid { get; set; }

    public string? TagType { get; set; }

    public decimal? UnitPrice { get; set; }

    public decimal? TotalLineAmount { get; set; }

    public int? ShippedFrom { get; set; }

    public int? FeastMovementid { get; set; }

    public string? Location { get; set; }

    public virtual WorkOrder? Workorder { get; set; }
}
