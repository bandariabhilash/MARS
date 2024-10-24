using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class CustomCriterion
{
    public int UniqueId { get; set; }

    public string? CategoryName { get; set; }

    public string? CategoryValue { get; set; }

    public string? CategoryType { get; set; }
}
