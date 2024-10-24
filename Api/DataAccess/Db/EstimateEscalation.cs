using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class EstimateEscalation
{
    public int Id { get; set; }

    public int Code { get; set; }

    public string? Name { get; set; }

    public bool? IsActive { get; set; }
}
