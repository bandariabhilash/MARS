using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class WorkorderBillingDetail
{
    public int BillingId { get; set; }

    public int? WorkorderId { get; set; }

    public string? BillingCode { get; set; }

    public int? Quantity { get; set; }

    public DateTime? EntryDate { get; set; }

    public string? Duration { get; set; }
}
