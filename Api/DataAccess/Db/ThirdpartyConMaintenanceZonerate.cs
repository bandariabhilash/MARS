using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class ThirdpartyConMaintenanceZonerate
{
    public int? ContractMaintenanceid { get; set; }

    public int? Techid { get; set; }

    public int ZoneRateid { get; set; }

    public string? Description { get; set; }

    public decimal? Rate { get; set; }

    public string? BasedOn { get; set; }
}
