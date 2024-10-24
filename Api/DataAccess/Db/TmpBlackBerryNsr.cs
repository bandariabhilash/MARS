using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class TmpBlackBerryNsr
{
    public int UniqueInvoiceId { get; set; }

    public int? WorkorderId { get; set; }

    public int? TechId { get; set; }

    public string? Reasion { get; set; }

    public string? Comments { get; set; }
}
