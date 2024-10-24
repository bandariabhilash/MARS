using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class VLastOneyearDispatch
{
    public int WorkorderId { get; set; }

    public string? WorkorderCallstatus { get; set; }

    public int? CustomerId { get; set; }

    public DateTime? ScheduleDate { get; set; }

    public int DealerId { get; set; }

    public string? CompanyName { get; set; }

    public string? Address1 { get; set; }

    public string? Address2 { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public DateTime? WorkorderEntryDate { get; set; }

    public string? PostalCode { get; set; }

    public int? ScheduleUserid { get; set; }
}
