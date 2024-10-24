using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class FbWorkOrderSku
{
    public int WorkOrderSkuid { get; set; }

    public int WorkorderId { get; set; }

    public string Sku { get; set; } = null!;

    public int? Qty { get; set; }
}
