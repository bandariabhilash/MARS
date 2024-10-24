using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class FberftransactionType
{
    public int Id { get; set; }

    public string? Type { get; set; }

    public int? TypeOrder { get; set; }

    public bool? IsActive { get; set; }
}
