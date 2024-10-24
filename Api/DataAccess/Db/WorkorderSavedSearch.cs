using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class WorkorderSavedSearch
{
    public string? SavedSearchName { get; set; }

    public int? Customerid { get; set; }

    public int? Erfid { get; set; }

    public int? WorkorderId { get; set; }

    public string? WorkorderCallStatus { get; set; }

    public string? SerialNumber { get; set; }

    public int? FollowupCall { get; set; }

    public string? AutoDispatch { get; set; }

    public string? WorkorderType { get; set; }

    public string? Priority { get; set; }

    public int? Fsr { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? ZipCode { get; set; }

    public int? CoverageZoneIndex { get; set; }

    public int? Tsm { get; set; }

    public string? TechType { get; set; }

    public string? ServiceCompany { get; set; }

    public string? Technician { get; set; }

    public int? Techid { get; set; }

    public int? Fsm { get; set; }

    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }

    public DateTime? ApptdateFrom { get; set; }

    public DateTime? ApptDateTo { get; set; }

    public int SavedSearchid { get; set; }

    public int? UserId { get; set; }

    public int? OriginalWorkOrderId { get; set; }
}
