using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class VoltageList
{
    public int VoltageId { get; set; }

    public string? Voltage { get; set; }

    public double? Active { get; set; }

    public double? Sequence { get; set; }
}
