using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class VRepeatServiceRepair
{
    public int WorkorderId { get; set; }

    public int? CustomerId { get; set; }

    public DateTime? WorkorderEntryDate { get; set; }

    public DateTime? WorkorderCloseDate { get; set; }

    public int? CallTypeid { get; set; }

    public string? Category { get; set; }

    public string? Manufacturer { get; set; }

    public string? Model { get; set; }

    public string? SerialNumber { get; set; }

    public int? Symptomid { get; set; }

    public int RepeatedEventId { get; set; }

    public DateTime? RepeatedEntryDate { get; set; }

    public DateTime? RepeatedCloseDate { get; set; }

    public int? RepeatedCallTypeId { get; set; }

    public string? RepeatedCategory { get; set; }

    public string? RepeatedManufacturer { get; set; }

    public string? RepeatedModel { get; set; }

    public string? RepeatSerialNumber { get; set; }

    public int? RepeatSymptomid { get; set; }
}
