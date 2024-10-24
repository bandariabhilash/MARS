using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class Fbexpendable
{
    public int ProductId { get; set; }

    public string? ProdNo { get; set; }

    public string? ModelNo { get; set; }

    public string? Description { get; set; }

    public string? UnitPrice { get; set; }
}
