using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class Vendor
{
    public int VendorId { get; set; }

    public string? VendorCode { get; set; }

    public string? VendorDescription { get; set; }

    public string? Phone { get; set; }

    public string? Fax { get; set; }

    public string? Contact { get; set; }

    public string? ApvendorNo { get; set; }

    public int? VendorActive { get; set; }
}
