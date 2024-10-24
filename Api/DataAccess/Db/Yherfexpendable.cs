using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class Yherfexpendable
{
    public string Erfno { get; set; } = null!;

    public int? ExpQuantity { get; set; }

    public int? TransactionTypeId { get; set; }

    public string? ExpProdNo { get; set; }

    public string? ExpModelNo { get; set; }

    public string? ExpDescription { get; set; }

    public decimal? UnitPrice { get; set; }

    public decimal? Extra { get; set; }

    public decimal? TotalPrice { get; set; }

    public int Sno { get; set; }
}
