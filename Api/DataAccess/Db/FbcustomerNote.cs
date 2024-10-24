using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class FbcustomerNote
{
    public int CustomerNotesId { get; set; }

    public int? CustomerId { get; set; }

    public string? Notes { get; set; }

    public int? UserId { get; set; }

    public string? UserName { get; set; }

    public DateTime? EntryDate { get; set; }

    public bool? IsActive { get; set; }

    public int? ParentId { get; set; }

    public virtual FbUserMaster? User { get; set; }
}
