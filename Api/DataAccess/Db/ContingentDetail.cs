using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class ContingentDetail
{
    public int Id { get; set; }

    public int ContingentId { get; set; }

    public string? Name { get; set; }

    public decimal? LaidInCost { get; set; }

    public decimal? CashSale { get; set; }

    public decimal? Rental { get; set; }

    public bool? IsActive { get; set; }
}
