using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class AllFbstatus
{
    public int FbstatusId { get; set; }

    public string? Fbstatus { get; set; }

    public short? Active { get; set; }

    public short? StatusSequence { get; set; }

    public string? StatusFor { get; set; }

    public int? SolutionId { get; set; }

    public string? Email { get; set; }
}
