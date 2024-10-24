using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class VEquipmentCount
{
    public int? EquipCount { get; set; }

    public int DealerId { get; set; }

    public string? CompanyName { get; set; }

    public string? BranchNumber { get; set; }

    public string? BranchName { get; set; }

    public int? WorkorderCalltypeid { get; set; }

    public string? WorkorderCalltypeDesc { get; set; }

    public bool? NoServiceRequired { get; set; }

    public DateTime? WorkorderCloseDate { get; set; }
}
