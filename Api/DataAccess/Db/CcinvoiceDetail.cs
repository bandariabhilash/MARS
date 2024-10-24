using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class CcinvoiceDetail
{
    public int Id { get; set; }

    public int? WorkorderId { get; set; }

    public decimal? TravelCost { get; set; }

    public decimal? LaborCost { get; set; }

    public decimal? PartsCost { get; set; }

    public decimal? PartsDiscount { get; set; }

    public double? SalesTax { get; set; }

    public string? TransactionId { get; set; }

    public bool? ManualCosts { get; set; }

    public virtual WorkOrder? Workorder { get; set; }
}
