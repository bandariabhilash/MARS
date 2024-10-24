using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class Fbreport
{
    public int ReportId { get; set; }

    public string? ReportName { get; set; }

    public string? ReportFilename { get; set; }

    public string? ReportCategory { get; set; }

    public string? DownloadName { get; set; }

    public string? DisplayFile { get; set; }

    public string? DescriptionFile { get; set; }

    public string? DownloadFile { get; set; }

    public short? Active { get; set; }

    public string? ReportType { get; set; }

    public virtual ICollection<FbuserReport> FbuserReports { get; set; } = new List<FbuserReport>();
}
