using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class TechHierarchy
{
    public int DealerId { get; set; }

    public string? CompanyName { get; set; }

    public string? Address1 { get; set; }

    public string? Address2 { get; set; }

    public string? Address3 { get; set; }

    public string? Address4 { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? PostalCode { get; set; }

    public string? Phone { get; set; }

    public string? Contact { get; set; }

    public string? SearchType { get; set; }

    public string? SearchDesc { get; set; }

    public string? DealerType { get; set; }

    public string? AreaCode { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public string? Fax { get; set; }

    public string? NextelPhone { get; set; }

    public int? SerialNoMan { get; set; }

    public string? ServiceRegion { get; set; }

    public string? EmailCc { get; set; }

    public string? LongAddressNumber { get; set; }

    public string? BusinessUnit { get; set; }

    public string? RimEmail { get; set; }

    public int? BranchAssociations { get; set; }

    public DateTime? LastModifiedByFtp { get; set; }

    public int? SendFax { get; set; }

    public int? SendNextel { get; set; }

    public int? SendEmail { get; set; }

    public string? FamilyAff { get; set; }

    public string? FamilyAffDesc { get; set; }

    public int? PricingParentId { get; set; }

    public string? PricingParentName { get; set; }

    public int? SendAutomaticEmail { get; set; }

    public int? SendDispatchConf { get; set; }

    public int? SendNsremail { get; set; }

    public string? AlternativePhone { get; set; }

    public int? AfterHoursOnly { get; set; }

    public int? PickFromZipRateTable { get; set; }

    public string? CustomerOwnNumber { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int? ModifiedUserId { get; set; }

    public string? BranchType { get; set; }

    public short? Unavailable { get; set; }

    public DateTime? UnavailableStartDate { get; set; }

    public DateTime? UnavailableEnddate { get; set; }

    public int? ReplacementTech { get; set; }

    public string? BranchNumber { get; set; }

    public string? BranchName { get; set; }

    public int AutoDispatch { get; set; }

    public string? RegionName { get; set; }

    public string? FieldServiceManager { get; set; }

    public string? RegionNumber { get; set; }

    public int? PrimaryTechId { get; set; }

    public bool? CustomerSpecificTech { get; set; }
}
