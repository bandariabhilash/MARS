using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class ZoneZip
{
    public int UniqueId { get; set; }

    public string? ZipCode { get; set; }

    public int? ZoneIndex { get; set; }

    public string? ZoneName { get; set; }
}
