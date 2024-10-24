using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class YheventNote
{
    public int UniqueId { get; set; }

    public int? EvenTid { get; set; }

    public int? UserId { get; set; }

    public DateTime? EnterDate { get; set; }

    public string? UserName { get; set; }

    public string? Notes { get; set; }

    public int? AutomaticNotes { get; set; }

    public int EventNotesId { get; set; }
}
