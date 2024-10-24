using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class WorkorderType
{
    public int CallTypeId { get; set; }

    public string? Description { get; set; }

    public int? Sequence { get; set; }

    public int? Active { get; set; }
}
