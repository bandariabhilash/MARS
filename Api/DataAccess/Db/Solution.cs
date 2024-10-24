using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class Solution
{
    public int SolutionId { get; set; }

    public string? Description { get; set; }

    public short? Sequence { get; set; }

    public int? Active { get; set; }

    public int? ColUpdated { get; set; }
}
