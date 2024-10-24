using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class WorkorderEquipmentRequested
{
    public int Assetid { get; set; }

    public int? CallTypeid { get; set; }

    public string? Category { get; set; }

    public string? Manufacturer { get; set; }

    public string? Model { get; set; }

    public string? Location { get; set; }

    public string? SerialNumber { get; set; }

    public int? Solutionid { get; set; }

    public string? Temperature { get; set; }

    public string? Settings { get; set; }

    public int? Systemid { get; set; }

    public int? Symptomid { get; set; }

    public bool? QualityIssue { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? WorkDescription { get; set; }

    public int? WorkorderId { get; set; }

    public string? WorkPerformedCounter { get; set; }

    public bool? NoPartsNeeded { get; set; }

    public string? CatalogId { get; set; }

    public int? FeastMovementid { get; set; }

    public bool? IsSlNumberImageExist { get; set; }

    public int? EquipmentId { get; set; }

    public string? Weight { get; set; }

    public string? Ratio { get; set; }

    public virtual WorkOrder? Workorder { get; set; }
}
