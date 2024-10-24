using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class TmpInvTransfer
{
    public int Transferid { get; set; }

    public DateTime? TransferDate { get; set; }

    public int? FromLoc { get; set; }

    public int? ToLoc { get; set; }

    public string? Comments { get; set; }
}
