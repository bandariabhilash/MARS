using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class FbactivityLog
{
    public int LogId { get; set; }

    public DateTime? LogDate { get; set; }

    public int? UserId { get; set; }

    public string? ErrorDetails { get; set; }

    public virtual FbUserMaster? User { get; set; }
}
