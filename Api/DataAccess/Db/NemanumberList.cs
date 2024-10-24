using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class NemanumberList
{
    public int NemaNumberId { get; set; }

    public string? NemaNumberDescription { get; set; }

    public double? Active { get; set; }

    public double? Sequence { get; set; }
}
