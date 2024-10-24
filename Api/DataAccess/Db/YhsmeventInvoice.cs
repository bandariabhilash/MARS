using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class YhsmeventInvoice
{
    public int? EventId { get; set; }

    public DateTime? StartDateTime { get; set; }

    public DateTime? ArrivalDateTime { get; set; }

    public DateTime? CompletionDateTime { get; set; }

    public int? InvoiceUserId { get; set; }

    public DateTime? InvoiceDate { get; set; }

    public int? ModifiedUserid { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? InvoiceNo { get; set; }

    public int? TechId { get; set; }

    public string? Technician { get; set; }

    public int InvoiceKey { get; set; }
}
