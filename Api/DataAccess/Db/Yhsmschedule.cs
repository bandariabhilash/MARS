using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class Yhsmschedule
{
    public int Scheduleid { get; set; }

    public int EventId { get; set; }

    public int Techid { get; set; }

    public DateTime? ScheduleDate { get; set; }

    public int? ScheduleUserid { get; set; }

    public DateTime? ModifiedScheduleDate { get; set; }

    public int? ModifiedScheduleUserid { get; set; }

    public int BranchId { get; set; }
}
