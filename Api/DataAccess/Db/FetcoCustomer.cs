using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class FetcoCustomer
{
    public int UniqueId { get; set; }

    public string CustomerNumber { get; set; } = null!;

    public string CustomerName { get; set; } = null!;

    public string? PaymentType { get; set; }
}
