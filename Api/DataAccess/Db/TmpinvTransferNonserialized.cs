using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class TmpinvTransferNonserialized
{
    public int Transferid { get; set; }

    public int? Catalogid { get; set; }

    public string? ManufNumber { get; set; }

    public string? Description { get; set; }

    public int? QtyTransfered { get; set; }

    public string? Bin { get; set; }

    public int? TechId { get; set; }

    public int? Van { get; set; }
}
