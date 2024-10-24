using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class PricingDetail
{
    public int Id { get; set; }

    public string? PricingEntityId { get; set; }

    public string? PricingEntityName { get; set; }

    public decimal? HourlyTravlRate { get; set; }

    public decimal? HourlyLablrRate { get; set; }

    public decimal? MilageRate { get; set; }

    public decimal? AfterHoursTravelRate { get; set; }

    public decimal? AfterHoursLaborRate { get; set; }

    public decimal? PartsDiscount { get; set; }

    public bool? AfterHoursRatesApply { get; set; }

    public decimal? AdditionalFee { get; set; }

    public int? PricingTypeId { get; set; }

    public bool? Approved3rdPartyUse { get; set; }

    public virtual PricingType? PricingType { get; set; }
}
