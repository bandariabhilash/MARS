using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class WorkorderImage
{
    public int WorkorderImageid { get; set; }

    public int? WorkorderDetailid { get; set; }

    public int? WorkorderId { get; set; }

    public byte[]? WorkorderPicture { get; set; }

    public string? WorkorderImagePath { get; set; }

    public int? AssetId { get; set; }

    public virtual WorkorderEquipment? Asset { get; set; }

    public virtual WorkOrder? Workorder { get; set; }
}
