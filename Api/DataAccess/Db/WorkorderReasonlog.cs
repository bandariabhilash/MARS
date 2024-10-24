using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class WorkorderReasonlog
{
    public int? WorkorderId { get; set; }

    public DateTime? OldAppointmentDate { get; set; }

    public DateTime? NewAppointmentDate { get; set; }

    public DateTime? EntryDate { get; set; }

    public int? Techid { get; set; }

    public int? Reasonid { get; set; }

    public string? LogDescription { get; set; }

    public string? Notes { get; set; }

    public int WorkorderLogid { get; set; }

    public string? ReasonFor { get; set; }

    public virtual WorkOrder? Workorder { get; set; }
}
