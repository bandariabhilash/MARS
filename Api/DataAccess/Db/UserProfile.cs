using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class UserProfile
{
    public int Techid { get; set; }

    public int? Branchid { get; set; }

    public DateTime? AcceptDate { get; set; }

    public DateTime? LastAcceptDate { get; set; }

    public string? TechType { get; set; }

    public string? UsrPassword { get; set; }

    public string? UserName { get; set; }

    public string? ApiKey { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? TokenKey { get; set; }

    public string? UserId { get; set; }

    public int? UserActive { get; set; }

    public string? DispatchEmails { get; set; }

    public bool? TechnicianAccount { get; set; }

    public bool? InvoicingAccount { get; set; }

    public bool IsInvoice { get; set; }
}
