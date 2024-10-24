using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class Sku
{
    public int Skuid { get; set; }

    public string? Sku1 { get; set; }

    public string? Description { get; set; }

    public short? SkuActive { get; set; }

    public string? VendorCode { get; set; }

    public decimal? Skucost { get; set; }

    public DateTime? LastModified { get; set; }

    public short? ColUpdated { get; set; }

    public string? Manufacturer { get; set; }

    public string? JmsItemNumber { get; set; }

    public string? VendItemNumber { get; set; }

    public string? EquipmentTag { get; set; }

    public string? Category { get; set; }
}
