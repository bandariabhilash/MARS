using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class WorkorderInstallationSurvey
{
    public int Installsurveyid { get; set; }

    public string? NemwNumber { get; set; }

    public string? ElectricalPhase { get; set; }

    public string? MachineAmperage { get; set; }

    public string? UnitFitSpace { get; set; }

    public string? Voltage { get; set; }

    public string? CounterUnitSpace { get; set; }

    public string? WaterLine { get; set; }

    public string? AssetLocation { get; set; }

    public string? Comments { get; set; }

    public int? WorkorderId { get; set; }

    public int? AssetId { get; set; }

    public virtual WorkorderEquipment? Asset { get; set; }

    public virtual WorkOrder? Workorder { get; set; }
}
