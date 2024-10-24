using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class BillingItem
{
    public int Id { get; set; }

    public string? BillingCode { get; set; }

    public string? BillingName { get; set; }

    public decimal? UnitPrice { get; set; }

    public DateTime? EntryDate { get; set; }

    public bool? IsActive { get; set; }
}
