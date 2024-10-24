using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class StateTax
{
    public int Id { get; set; }

    public string? State { get; set; }

    public string? ZipCode { get; set; }

    public string? TaxRegionName { get; set; }

    public double? EstimatedCombinedRate { get; set; }

    public double? StateRate { get; set; }

    public double? EstimatedCountyRate { get; set; }

    public double? EstimatedCityRate { get; set; }

    public double? EstimatedSpecialRate { get; set; }
}
