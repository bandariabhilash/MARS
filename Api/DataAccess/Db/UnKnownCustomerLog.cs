using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class UnKnownCustomerLog
{
    public int CustomerLogId { get; set; }

    public int? OldCustomerId { get; set; }

    public int? NewCustomerId { get; set; }

    public string? ModifiedUserName { get; set; }

    public int? ModifiedUserId { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
