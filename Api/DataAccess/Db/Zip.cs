using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class Zip
{
    public string? Zip1 { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? County { get; set; }

    public string? Country { get; set; }

    public string? AreaCode { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public short? PreferredCity { get; set; }

    public int? TimeZone { get; set; }

    public string? DayLightSaving { get; set; }

    public string? Fipsstate { get; set; }

    public string? Fipscounty { get; set; }

    public string? Fipscity { get; set; }

    public int UniqueId { get; set; }

    public string? TimeZoneName { get; set; }
}
