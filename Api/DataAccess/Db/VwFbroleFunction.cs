using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class VwFbroleFunction
{
    public int RoleId { get; set; }

    public int FunctionId { get; set; }

    public int? CanCreate { get; set; }

    public int? CanUpdate { get; set; }

    public int? CanView { get; set; }

    public int? CanExport { get; set; }

    public int? CanEmail { get; set; }

    public int? ParentFunctionId { get; set; }

    public string? FunctionName { get; set; }

    public int? FunctionLevel { get; set; }

    public bool? IsActive { get; set; }

    public int? RoleFunctionId { get; set; }
}
