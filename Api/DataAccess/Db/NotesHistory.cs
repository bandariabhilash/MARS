using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class NotesHistory
{
    public string? ErfId { get; set; }

    public int? WorkorderId { get; set; }

    public int? FeastMovementId { get; set; }

    public int? Userid { get; set; }

    public string? UserName { get; set; }

    public string? Notes { get; set; }

    public short? AutomaticNotes { get; set; }

    public int NotesId { get; set; }

    public DateTime? EntryDate { get; set; }

    public int? NonServiceWorkorderId { get; set; }

    public short? IsDispatchNotes { get; set; }

    public virtual NonServiceworkorder? NonServiceWorkorder { get; set; }

    public virtual WorkOrder? Workorder { get; set; }
}
