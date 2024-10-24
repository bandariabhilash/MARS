using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class Fberfexpendable
{
    public int ErfexpendableId { get; set; }

    public string? Erfid { get; set; }

    public int? WorkOrderId { get; set; }

    public string? ModelNo { get; set; }

    public int? Quantity { get; set; }

    public string? ProdNo { get; set; }

    public string? TransactionType { get; set; }

    public decimal? UnitPrice { get; set; }

    public string? Extra { get; set; }

    public string? Description { get; set; }

    public int? ContingentCategoryId { get; set; }

    public int? ContingentCategoryTypeId { get; set; }

    public string? UsingBranch { get; set; }

    public decimal? LaidInCost { get; set; }

    public decimal? RentalCost { get; set; }

    public decimal? TotalCost { get; set; }

    public string? InternalOrderType { get; set; }

    public string? VendorOrderType { get; set; }

    public string? EquipmentType { get; set; }

    public string? Substitution { get; set; }

    public virtual Erf? Erf { get; set; }

    public virtual WorkOrder? WorkOrder { get; set; }
}
