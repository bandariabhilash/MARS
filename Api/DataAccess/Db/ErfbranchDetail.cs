using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class ErfbranchDetail
{
    public int Id { get; set; }

    public string? Region { get; set; }

    public string? District { get; set; }

    public string? Branch { get; set; }

    public string? BranchName { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? PostalCode { get; set; }

    public bool? IsActive { get; set; }
}
