using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class WaterLineList
{
    public int WaterLineId { get; set; }

    public string? WaterLine { get; set; }

    public double? Active { get; set; }

    public double? Sequence { get; set; }
}
