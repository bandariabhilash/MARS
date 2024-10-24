using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class TechOnCall
{
    public int TechOnCallId { get; set; }

    public int? TechId { get; set; }

    public DateTime? ScheduleDate { get; set; }

    public decimal? ScheduleStartTime { get; set; }

    public decimal? ScheduleEndTime { get; set; }

    public int? EntryUserId { get; set; }

    public string? EntryUserName { get; set; }

    public int? ModifiedUserId { get; set; }

    public string? ModifiedUserName { get; set; }

    public DateTime? ScheduleEndDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public DateTime? ScheduleCreatedDate { get; set; }
}
