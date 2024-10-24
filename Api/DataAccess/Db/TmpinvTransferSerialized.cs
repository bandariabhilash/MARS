using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class TmpinvTransferSerialized
{
    public int Transferid { get; set; }

    public int? Catalogid { get; set; }

    public int? WorkorderId { get; set; }

    public bool? Sealed { get; set; }

    public string? EquipmentStatus { get; set; }

    public string? Model { get; set; }

    public string? SerialNumber { get; set; }

    public string? Manufacturer { get; set; }

    public string? FileName { get; set; }

    public string? Bin { get; set; }

    public int? TechId { get; set; }

    public int? Van { get; set; }
}
