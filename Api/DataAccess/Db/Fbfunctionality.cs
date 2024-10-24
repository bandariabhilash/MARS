using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class Fbfunctionality
{
    public int FunctionId { get; set; }

    public int? ParentFunctionId { get; set; }

    public string? FunctionName { get; set; }

    public int? FunctionLevel { get; set; }

    public bool? IsActive { get; set; }

    public int? Order { get; set; }
}
