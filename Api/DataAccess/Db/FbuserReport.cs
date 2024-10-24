using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class FbuserReport
{
    public int UserReportId { get; set; }

    public int UserId { get; set; }

    public int ReportId { get; set; }

    public virtual Fbreport Report { get; set; } = null!;

    public virtual FbUserMaster User { get; set; } = null!;
}
