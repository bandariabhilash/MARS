using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class ErforderType
{
    public int OrderTypeId { get; set; }

    public string? OrderType { get; set; }

    public bool? IsActive { get; set; }
}
