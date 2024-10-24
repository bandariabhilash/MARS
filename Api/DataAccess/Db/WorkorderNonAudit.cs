using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class WorkorderNonAudit
{
    public int NonAuditid { get; set; }

    public int? WorkorderId { get; set; }

    public int? CustomerId { get; set; }

    public int? Techid { get; set; }

    public string? Category { get; set; }

    public string? Equipment { get; set; }

    public string? Model { get; set; }

    public string? Location { get; set; }

    public string? SerialNumber { get; set; }

    public virtual WorkOrder? Workorder { get; set; }
}
