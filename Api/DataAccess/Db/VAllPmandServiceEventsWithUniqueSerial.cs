using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class VAllPmandServiceEventsWithUniqueSerial
{
    public int WorkorderId { get; set; }

    public string? WorkorderCallstatus { get; set; }

    public int? CustomerId { get; set; }

    public DateTime? WorkorderEntryDate { get; set; }

    public DateTime? WorkorderCloseDate { get; set; }

    public int? CallTypeid { get; set; }

    public string? Category { get; set; }

    public string? Manufacturer { get; set; }

    public string? Model { get; set; }

    public string? SerialNumber { get; set; }

    public int? Symptomid { get; set; }
}
