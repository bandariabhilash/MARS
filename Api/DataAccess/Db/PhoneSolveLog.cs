using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class PhoneSolveLog
{
    public int PhoneSolveLogId { get; set; }

    public int WorkorderId { get; set; }

    public int PhoneSolveId { get; set; }

    public int? TechId { get; set; }

    public DateTime AttemptedDate { get; set; }

    public virtual WorkOrder Workorder { get; set; } = null!;
}
