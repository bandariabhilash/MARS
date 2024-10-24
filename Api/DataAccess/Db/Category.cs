using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class Category
{
    public int CategoryId { get; set; }

    public string? CategoryDesc { get; set; }

    public int? Active { get; set; }

    public int? ColUpdated { get; set; }

    public string? CategoryCode { get; set; }
}
