using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class Erf
{
    public string ErfId { get; set; } = null!;

    public int? WorkorderId { get; set; }

    public int? CustomerId { get; set; }

    public string? CustomerName { get; set; }

    public string? CustomerAddress { get; set; }

    public string? CustomerCity { get; set; }

    public string? CustomerState { get; set; }

    public string? CustomerZipCode { get; set; }

    public string? CustomerMainContactName { get; set; }

    public string? CustomerPhone { get; set; }

    public string? CustomerPhoneExtn { get; set; }

    public string? CustomerMainEmail { get; set; }

    public int? FeastMovementId { get; set; }

    public short? ChannelId { get; set; }

    public string? SalesPerson { get; set; }

    public int? EntryUserId { get; set; }

    public DateTime? EntryDate { get; set; }

    public int? ModifiedUserId { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public short? ReasonId { get; set; }

    public DateTime? DateOnErf { get; set; }

    public DateTime? DateErfreceived { get; set; }

    public DateTime? DateErfprocessed { get; set; }

    public DateTime? TimeErfprocessed { get; set; }

    public DateTime? OriginalRequestedDate { get; set; }

    public string? InstallLocation { get; set; }

    public int? TimeZone { get; set; }

    public string? DayLightSaving { get; set; }

    public DateTime? EquipEtadate { get; set; }

    public int? ShipToJde { get; set; }

    public string? SiteReady { get; set; }

    public DateTime? JdeProcessDate { get; set; }

    public int? ShipfromJde { get; set; }

    public string? UserName { get; set; }

    public string? TransType { get; set; }

    public string? CustomerPo { get; set; }

    public string? CmUser { get; set; }

    public DateTime? CmReceviedDate { get; set; }

    public DateTime? DateReceived { get; set; }

    public string? LoanSale { get; set; }

    public string? HoursofOperation { get; set; }

    public string? Phone { get; set; }

    public bool? IsSiteReady { get; set; }

    public string? OrderType { get; set; }

    public string? ShipToBranch { get; set; }

    public string? Erfstatus { get; set; }

    public decimal? TotalNsv { get; set; }

    public decimal? CurrentEqp { get; set; }

    public decimal? AdditionalEqp { get; set; }

    public string? ApprovalStatus { get; set; }

    public decimal? CurrentNsv { get; set; }

    public string? ContributionMargin { get; set; }

    public int? GroupId { get; set; }

    public string? BulkUploadResult { get; set; }

    public string? UploadError { get; set; }

    public string? CashSaleStatus { get; set; }

    public string? Tracking { get; set; }

    public virtual ICollection<Fberfequipment> Fberfequipments { get; set; } = new List<Fberfequipment>();

    public virtual ICollection<Fberfexpendable> Fberfexpendables { get; set; } = new List<Fberfexpendable>();
}
