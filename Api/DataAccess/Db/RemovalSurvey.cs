using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class RemovalSurvey
{
    public int RemovalSurveyId { get; set; }

    public int? WorkorderId { get; set; }

    public int? JmsownedMachines { get; set; }

    public DateTime? RemovalDate { get; set; }

    public string? RemoveAllMachines { get; set; }

    public string? RemovalReason { get; set; }

    public string? BeveragesSupplier { get; set; }
}
