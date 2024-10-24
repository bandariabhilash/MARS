using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class ErfWorkorderLog
{
    public string ErfId { get; set; } = null!;

    public int WorkorderId { get; set; }
}
