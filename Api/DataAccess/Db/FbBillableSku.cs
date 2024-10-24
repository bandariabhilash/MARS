using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class FbBillableSku
{
    public string Sku { get; set; } = null!;

    public string? Skudescription { get; set; }

    public decimal? UnitPrice { get; set; }

    public bool? IsActive { get; set; }
}
