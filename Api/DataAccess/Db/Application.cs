using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class Application
{
    public int ApplicationId { get; set; }

    public string? ApplicationName { get; set; }

    public int? OrderId { get; set; }

    public virtual ICollection<UserApplication> UserApplications { get; set; } = new List<UserApplication>();
}
