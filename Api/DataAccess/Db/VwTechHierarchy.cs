using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class VwTechHierarchy
{
    public string? Rsmname { get; set; }

    public string? Rsmphone { get; set; }

    public string? Esmname { get; set; }

    public string? Esmphone { get; set; }

    public string? Dsmname { get; set; }

    public string? Dsmphone { get; set; }

    public string? Branch { get; set; }

    public string? RegionName { get; set; }

    public int? Rsmid { get; set; }

    public int? Esmid { get; set; }

    public int? Dsmid { get; set; }

    public string? PreferredProvider { get; set; }

    public string? ProviderPhone { get; set; }

    public string? PricingParent { get; set; }

    public int TechId { get; set; }

    public string? TechZip { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public string? TechType { get; set; }

    public string? TechTypeDesc { get; set; }

    public string? TechEmail { get; set; }

    public string? RegionNumber { get; set; }

    public string? BranchNumber { get; set; }

    public string? BranchName { get; set; }

    public string? AreaCode { get; set; }

    public string? SearchType { get; set; }
}
