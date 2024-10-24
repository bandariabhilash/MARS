using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class State
{
    public string StateCode { get; set; } = null!;

    public string? StateName { get; set; }

    public decimal? TaxPercent { get; set; }
}
