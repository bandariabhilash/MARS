using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class WorkorderSchedule
{
    public int Scheduleid { get; set; }

    public int? WorkorderId { get; set; }

    public int? Techid { get; set; }

    public DateTime? ScheduleDate { get; set; }

    public int? ScheduleUserid { get; set; }

    public DateTime? ModifiedScheduleDate { get; set; }

    public int? ModifiedScheduleUserid { get; set; }

    public DateTime? EntryDate { get; set; }

    public int? ServiceCenterId { get; set; }

    public short? PrimaryTech { get; set; }

    public string? TechName { get; set; }

    public string? TechPhone { get; set; }

    public string? ServiceCenterName { get; set; }

    public int? Fsmid { get; set; }

    public string? Fsmname { get; set; }

    public string? Fsmphone { get; set; }

    public int? Tsmid { get; set; }

    public string? Tsmname { get; set; }

    public string? Tsmemail { get; set; }

    public string? Tsmphone { get; set; }

    public string? Fsmemail { get; set; }

    public int? TeamLeadId { get; set; }

    public string? TeamLeadName { get; set; }

    public string? AssignedStatus { get; set; }

    public short? AssistTech { get; set; }

    public DateTime? WsArrivalDateTime { get; set; }

    public DateTime? WsCompletionDateTime { get; set; }

    public decimal? WsMileage { get; set; }

    public string? WsTravelTime { get; set; }

    public DateTime? WsAppointmentDate { get; set; }

    public bool? IsTechClosed { get; set; }

    public DateTime? EventScheduleDate { get; set; }

    public string? ScheduleContactName { get; set; }

    public virtual WorkOrder? Workorder { get; set; }
}
