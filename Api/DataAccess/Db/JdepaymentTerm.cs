using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class JdepaymentTerm
{
    public int UniqueId { get; set; }

    public string? PaymentTerm { get; set; }

    public string? Description { get; set; }
}
