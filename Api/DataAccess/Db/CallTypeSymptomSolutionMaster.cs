using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class CallTypeSymptomSolutionMaster
{
    public int? CallTypeId { get; set; }

    public int? SymptomId { get; set; }

    public int? SolutionId { get; set; }

    public int? Active { get; set; }

    public int Id { get; set; }

    public int? ColUpdated { get; set; }
}
