using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class TmpfeastPart
{
    public double? CatalogId { get; set; }

    public string? VersionNumber { get; set; }

    public string? Manufacturer { get; set; }

    public string? Model { get; set; }

    public string? Description { get; set; }

    public string? JmsItemNumber { get; set; }

    public string? VendItemNumber { get; set; }

    public DateTime? LastEditedDate { get; set; }

    public string? EquipmentTag { get; set; }

    public string? Active { get; set; }

    public double? Price { get; set; }

    public string? Category { get; set; }

    public short? SkuActive { get; set; }
}
