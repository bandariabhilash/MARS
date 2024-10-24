using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class NonServiceworkorder
{
    public int NonServiceId { get; set; }

    public int WorkOrderId { get; set; }

    public int? CustomerId { get; set; }

    public string? CallReason { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? CustomerCity { get; set; }

    public string? CustomerState { get; set; }

    public string? CustomerZipCode { get; set; }

    public bool? IsAutoDispatched { get; set; }

    public string? CallerName { get; set; }

    public string? CallBack { get; set; }

    public bool? IsUnknownWorkOrder { get; set; }

    public string? NonServiceEventStatus { get; set; }

    public DateTime? CloseDate { get; set; }

    public string? EmailSentTo { get; set; }

    public string? MainContactName { get; set; }

    public string? PhoneNumber { get; set; }

    public int? ClosedBy { get; set; }

    public string? ResolutionCallerName { get; set; }

    public virtual ICollection<NotesHistory> NotesHistories { get; set; } = new List<NotesHistory>();
}
