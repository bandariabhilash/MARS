using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class WorkorderServiceClaim
{
    public int? WorkorderId { get; set; }

    public int? Assetid { get; set; }

    public int? Techid { get; set; }

    public int ServiceClaimid { get; set; }

    public virtual WorkorderEquipment? Asset { get; set; }
}
