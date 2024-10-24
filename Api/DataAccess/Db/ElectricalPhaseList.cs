using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class ElectricalPhaseList
{
    public int ElectricalPhaseId { get; set; }

    public string? ElectricalPhase { get; set; }

    public double? Active { get; set; }

    public double? Sequence { get; set; }
}
