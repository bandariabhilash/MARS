using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class FeastMovement
{
    public int? Feastmovementid { get; set; }

    public int? WorkorderId { get; set; }

    public string? Erfid { get; set; }

    public string? FeastmovementState { get; set; }

    public string? ShipMethod { get; set; }

    public string? ShipCarrier { get; set; }

    public string? TrackingNumber { get; set; }

    public string? Comments { get; set; }

    public int UniqueId { get; set; }
}
