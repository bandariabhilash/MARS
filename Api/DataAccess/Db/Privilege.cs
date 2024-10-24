using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class Privilege
{
    public int PrivilegeId { get; set; }

    public string? PrivilegeType { get; set; }

    public virtual ICollection<UserApplication> UserApplications { get; set; } = new List<UserApplication>();
}
