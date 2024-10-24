using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class Fbsku
{
    public int Skuid { get; set; }

    public string? Sku { get; set; }

    public string? Description { get; set; }

    public bool? Skuactive { get; set; }

    public string? Weight { get; set; }

    public string? Skucost { get; set; }

    public string? VendorCode { get; set; }

    public string? ShortProductNumber { get; set; }

    public string? SystemSource { get; set; }

    public string? Ben02ItemNumber { get; set; }

    public string? VendorNo { get; set; }

    public string? E1number { get; set; }
}
