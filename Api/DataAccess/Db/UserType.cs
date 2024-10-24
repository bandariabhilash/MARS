using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class UserType
{
    public int TypeId { get; set; }

    public string? TypeName { get; set; }

    public virtual ICollection<FbUserMaster> FbUserMasters { get; set; } = new List<FbUserMaster>();
}
