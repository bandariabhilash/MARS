using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class UserApplication
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? ApplicationId { get; set; }

    public int? PrivilegeId { get; set; }

    public virtual Application? Application { get; set; }

    public virtual Privilege? Privilege { get; set; }

    public virtual FbUserMaster? User { get; set; }
}
