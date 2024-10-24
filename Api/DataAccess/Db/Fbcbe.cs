using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class Fbcbe
{
    public int Id { get; set; }

    public string? ItemNumber { get; set; }

    public string? ItemDescription { get; set; }

    public string? SerialNumber { get; set; }

    public string? AssetStatus { get; set; }

    public string? CurrentLocation { get; set; }

    public int? CurrentCustomerId { get; set; }

    public string? CurrentCustomerName { get; set; }

    public DateTime? TransDate { get; set; }

    public decimal? InitialValue { get; set; }

    public DateTime? InitialDate { get; set; }

    public string? CurrentGlcode { get; set; }

    public string? CurrentGlobject { get; set; }
}
