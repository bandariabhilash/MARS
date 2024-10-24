using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class FbRole
{
    public int RoleId { get; set; }

    public string? RoleName { get; set; }

    public int? IsActive { get; set; }

    public virtual ICollection<FbUserMaster> FbUserMasters { get; set; } = new List<FbUserMaster>();
}
