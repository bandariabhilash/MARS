using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class BranchEsm
{
    public int UniqueId { get; set; }

    public string? BranchNo { get; set; }

    public string? Esmname { get; set; }

    public string? BranchName { get; set; }
}
