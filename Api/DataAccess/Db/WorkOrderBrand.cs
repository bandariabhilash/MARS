using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class WorkOrderBrand
{
    public int UniqueId { get; set; }

    public int? WorkorderId { get; set; }

    public int? BrandId { get; set; }

    public virtual WorkOrder? Workorder { get; set; }
}
