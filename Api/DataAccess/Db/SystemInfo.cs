using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class SystemInfo
{
    public int SystemId { get; set; }

    public string? Description { get; set; }

    public short? Sequence { get; set; }

    public int? Active { get; set; }
}
