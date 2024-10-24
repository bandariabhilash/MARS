using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class FbclosurePart
{
    public int SkuId { get; set; }

    public string? EntryNumber { get; set; }

    public string? ItemNo { get; set; }

    public string? VendorNo { get; set; }

    public string? Description { get; set; }

    public string? Supplier { get; set; }

    public string? OrderSource { get; set; }

    public string? Version { get; set; }

    public bool? SkuActive { get; set; }
}
