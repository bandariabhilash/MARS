using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class Contingent
{
    public int ContingentId { get; set; }

    public string? ContingentName { get; set; }

    public string? ContingentType { get; set; }

    public bool? IsActive { get; set; }
}
