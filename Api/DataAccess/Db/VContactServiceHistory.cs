using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class VContactServiceHistory
{
    public int WorkorderId { get; set; }

    public string? WorkorderCallstatus { get; set; }

    public int? CustomerId { get; set; }

    public string? Category { get; set; }

    public string? Model { get; set; }

    public string? Manufacturer { get; set; }

    public string? Location { get; set; }

    public string? SerialNumber { get; set; }

    public string? CallTypeDesc { get; set; }

    public string? SymptomDesc { get; set; }

    public int SymptomId { get; set; }

    public int CallTypeId { get; set; }

    public DateTime? WorkorderEntryDate { get; set; }

    public int? Solutionid { get; set; }

    public int? Systemid { get; set; }

    public string? Description { get; set; }

    public int Assetid { get; set; }
}
