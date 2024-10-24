using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class Yhsmevent
{
    public int UniqueId { get; set; }

    public int EventId { get; set; }

    public int ContactId { get; set; }

    public string? FulfillmentStatus { get; set; }

    public int? EntryUserId { get; set; }

    public DateTime? EntryDate { get; set; }

    public int? CloseUserId { get; set; }

    public DateTime? CloseDate { get; set; }

    public int? ModifiedUserId { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? Erfno { get; set; }

    public int? EquipCount { get; set; }

    public int? CallTypeId { get; set; }

    public string? CallTypeDesc { get; set; }

    public int? SpawnEvent { get; set; }

    public int? TimeZone { get; set; }

    public string? DayLightSaving { get; set; }

    public string? ClosureConfirmationNo { get; set; }

    public int? Fsm { get; set; }

    public string? SeriviceLevelCode { get; set; }

    public DateTime? AppointmentDate { get; set; }

    public string? Recallevent { get; set; }

    public int? NewPriorityCode { get; set; }

    public string? EventContact { get; set; }

    public int? CallPriority { get; set; }

    public string? ContactPhone { get; set; }

    public string? CallerName { get; set; }

    public int? ProjectNumber { get; set; }
}
