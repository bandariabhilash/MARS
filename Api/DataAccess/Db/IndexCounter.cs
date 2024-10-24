using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class IndexCounter
{
    public int Indexid { get; set; }

    public string? IndexName { get; set; }

    public int? IndexValue { get; set; }
}
