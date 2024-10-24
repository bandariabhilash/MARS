using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class YhcompassSurvey
{
    public int EventId { get; set; }

    public int AssetKey { get; set; }

    public int? NemaNumberId { get; set; }

    public int? Ampsid { get; set; }

    public int? VoltageId { get; set; }

    public int? WaterLineId { get; set; }

    public int? ElectricalPhaseId { get; set; }

    public int? UnitSpaceId { get; set; }

    public int? UnitWeightId { get; set; }

    public int CompassSurveyId { get; set; }

    public int? Removed { get; set; }

    public string? SurveyLocation { get; set; }

    public string? NemwNumber { get; set; }

    public string? ElectricalPhase { get; set; }

    public string? MachineAmperage { get; set; }

    public string? UnitFitSpace { get; set; }

    public string? Voltage { get; set; }

    public string? CounterUnitSpace { get; set; }

    public string? WaterLine { get; set; }

    public string? AssetLocation { get; set; }
}
