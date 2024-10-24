using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class Ampslist
{
    public int Ampsid { get; set; }

    public string? Ampsdescription { get; set; }

    public int? Active { get; set; }

    public int? Sequence { get; set; }
}
