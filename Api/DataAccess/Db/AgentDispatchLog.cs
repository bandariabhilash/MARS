using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class AgentDispatchLog
{
    public int Id { get; set; }

    public DateTime? Tdate { get; set; }

    public int? UserId { get; set; }

    public int? WorkorderId { get; set; }

    public string? UserName { get; set; }
}
