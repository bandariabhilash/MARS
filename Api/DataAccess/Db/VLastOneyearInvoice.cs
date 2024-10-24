using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class VLastOneyearInvoice
{
    public int WorkorderId { get; set; }

    public string? WorkorderCallstatus { get; set; }

    public int? CustomerId { get; set; }

    public DateTime? WorkorderEntryDate { get; set; }

    public string? InvoiceNo { get; set; }

    public DateTime? InvoiceDate { get; set; }

    public DateTime? StartDateTime { get; set; }

    public DateTime? ArrivalDateTime { get; set; }

    public DateTime? CompletionDateTime { get; set; }

    public int? ResponsibleTechid { get; set; }

    public string? ResponsibleTechName { get; set; }
}
