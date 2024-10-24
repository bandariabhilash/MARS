using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class TmpBlackBerryScfpartsInfo
{
    public int UniquePartsId { get; set; }

    public int? WorkorderId { get; set; }

    public int? TechId { get; set; }

    public string? EntryNo { get; set; }

    public string? ItemNo { get; set; }

    public string? VendorNo { get; set; }

    public string? Description { get; set; }

    public DateTime? EntryDate { get; set; }

    public int? Skuid { get; set; }

    public int? Quantity { get; set; }

    public int? Pid { get; set; }

    public string? Sku { get; set; }

    public short? RecExists { get; set; }

    public int? AssetId { get; set; }
}
