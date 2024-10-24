using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class VContact
{
    public string? CompanyName { get; set; }

    public string? Address1 { get; set; }

    public string? Address2 { get; set; }

    public string? Address3 { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? PostalCode { get; set; }

    public string? Phone { get; set; }

    public string? Division { get; set; }

    public string? DivisionDesc { get; set; }

    public string? District { get; set; }

    public string? DistrictDesc { get; set; }

    public string? FamilyAff { get; set; }

    public string? FamilyAffDesc { get; set; }

    public string? PricingParent { get; set; }

    public string? PricingParentName { get; set; }

    public string? AcctRep { get; set; }

    public string? AcctRepDesc { get; set; }

    public int? RouteNumber { get; set; }

    public int? Distributor { get; set; }

    public string? DistributorName { get; set; }

    public string? DeliveryMethod { get; set; }

    public string? DeliveryDesc { get; set; }

    public string? SearchType { get; set; }

    public string? SearchDesc { get; set; }

    public string? BusinessUnit { get; set; }

    public DateTime? DateCreated { get; set; }

    public DateTime? LastModified { get; set; }

    public string? AreaCode { get; set; }

    public short? ContactActive { get; set; }

    public string? LongAddressNumber { get; set; }

    public DateTime? LastModifiedByFtp { get; set; }

    public int? AuditFlow { get; set; }

    public int? SendInvoiceTo { get; set; }

    public int? Fsmjde { get; set; }

    public string? FieldServiceManager { get; set; }

    public string? ServiceLevelCode { get; set; }

    public DateTime? AnniversaryDate { get; set; }

    public string? TierDesc { get; set; }

    public int? RemainingFreeCalls { get; set; }

    public int? Ccmjde { get; set; }

    public int? Rccmjde { get; set; }

    public string? CategoryCode { get; set; }

    public string? OperatingUnit { get; set; }

    public string? Route { get; set; }

    public string? Branch { get; set; }

    public string? _1099reporting { get; set; }

    public string? Chain { get; set; }

    public string? BrewmaticAgentCode { get; set; }

    public string? Ntr { get; set; }

    public string? DelDayOrFob { get; set; }

    public string? RouteCode { get; set; }

    public string? ZoneNumber { get; set; }

    public string? MailingName { get; set; }

    public string? FirstName { get; set; }

    public string? MiddleName { get; set; }

    public string? LastName { get; set; }

    public int? FbproviderId { get; set; }

    public int? Nofbpspemails { get; set; }

    public DateTime? LastAutoEmail { get; set; }

    public string? CustomerRegion { get; set; }

    public string? RegionNumber { get; set; }

    public string? CustomerBranch { get; set; }

    public string? PricingParentId { get; set; }

    public string? PricingParentDesc { get; set; }

    public string? LastSaleDate { get; set; }

    public string? Email { get; set; }

    public string? CustomerSpecialInstructions { get; set; }

    public string? PhoneWithAreaCode { get; set; }

    public int? IsUnknownUser { get; set; }

    public int ContactId { get; set; }

    public int? DealerId { get; set; }

    public string? Expr1 { get; set; }

    public string? EmailCc { get; set; }

    public string? Expr2 { get; set; }

    public string? RimEmail { get; set; }

    public int? BranchAssociations { get; set; }

    public string? Expr3 { get; set; }
}
