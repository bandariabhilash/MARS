using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class UserRefreshtoken
{
    public int Uniqueid { get; set; }

    public string? Refreshtoken { get; set; }

    public int? Userid { get; set; }

    public DateTime? Refreshtokenexpirytime { get; set; }

    public virtual FbUserMaster? User { get; set; }
}
