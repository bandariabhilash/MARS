using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class PricingType
{
    public int PricingTypeId { get; set; }

    public string? PricingTypeName { get; set; }

    public virtual ICollection<PricingDetail> PricingDetails { get; set; } = new List<PricingDetail>();
}
