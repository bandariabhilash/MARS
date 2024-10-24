using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class TechSchedule
{
    public int TechScheduleId { get; set; }

    public int? TechId { get; set; }

    public string? Availability { get; set; }

    public DateTime? ScheduleDate { get; set; }

    public decimal? ScheduleStartTime { get; set; }

    public decimal? ScheduleEndTime { get; set; }

    public int? WorkOrderId { get; set; }

    public int? EntryUserId { get; set; }

    public string? EntryUserName { get; set; }

    public int? ModifiedUserId { get; set; }

    public string? ModifiedUserName { get; set; }

    public string? AppointmentSubject { get; set; }

    public int? ScheduleStartEndId { get; set; }

    public int? ReplaceTech { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public DateTime? ScheduleCreatedDate { get; set; }

    public virtual WorkOrder? WorkOrder { get; set; }
}
