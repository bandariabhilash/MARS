using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class WorkorderStatusLog
{
    public int WorkorderStatusLogId { get; set; }

    public int WorkorderId { get; set; }

    public string StatusFrom { get; set; } = null!;

    public string StatusTo { get; set; } = null!;

    public DateTime StausChangeDate { get; set; }

    public virtual WorkOrder Workorder { get; set; } = null!;
}
