using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class TmpSerialNoreport
{
    public int EventId { get; set; }

    public string? FulfillmentStatus { get; set; }

    public DateTime? EntryDate { get; set; }

    public DateTime? CloseDate { get; set; }

    public int ContactId { get; set; }

    public string? CompanyName { get; set; }

    public int? CallTypeId { get; set; }

    public string? CallTypeDesc { get; set; }

    public string? SerialNo { get; set; }

    public string? ProductNo { get; set; }

    public string? ProductDesc1 { get; set; }

    public string? Manufacturer { get; set; }

    public string? ManufacturerDesc { get; set; }

    public int? NoService { get; set; }

    public string? SearchType { get; set; }

    public string? SearchDesc { get; set; }
}
