using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class FbcallReason
{
    public int SourceId { get; set; }

    public string? SourceCode { get; set; }

    public string? Description { get; set; }

    public DateOnly? DateActive { get; set; }

    public DateOnly? DateInactive { get; set; }

    public DateOnly? EntryDate { get; set; }

    public bool? EmailFsm { get; set; }

    public bool? EmailCcm { get; set; }

    public string? AdditionalEmail { get; set; }

    public int? SalesCall { get; set; }

    public int? OrderBy { get; set; }

    public bool? EmailRegional { get; set; }

    public bool? EmailSalesManager { get; set; }

    public bool? EmailRsr { get; set; }
}
