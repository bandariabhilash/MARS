using System;
using System.Collections.Generic;

namespace DataAccess.Db;

public partial class Invoice
{
    public int InvoiceUniqueid { get; set; }

    public string Invoiceid { get; set; } = null!;

    public int? WorkorderId { get; set; }

    public string? CustomerName { get; set; }

    public string? ServiceLocation { get; set; }

    public DateTime? WorkorderCompletionDate { get; set; }

    public string? InvoiceStatus { get; set; }

    public DateTime? InvoiceSubmitDate { get; set; }

    public decimal? SubmitAmount { get; set; }

    public DateTime? PaymentCreated { get; set; }

    public decimal? PaymentAmtApproved { get; set; }

    public string? CheckNumber { get; set; }

    public int? Mileage { get; set; }

    public int? TravelTimeInSecs { get; set; }

    public int? StandardTravelSecs { get; set; }

    public int? OverTimeTravelSecs { get; set; }

    public short? RoundTripIncluded { get; set; }

    public decimal? LaborHours { get; set; }

    public decimal? StandardLabor { get; set; }

    public decimal? OvertimeLabor { get; set; }

    public int? ZoneRateid { get; set; }

    public decimal? AdditionalCharge { get; set; }

    public string? AdditionalChargeDescription { get; set; }

    public string? InvoiceComments { get; set; }

    public decimal? PartsTotal { get; set; }

    public decimal? LaborTotal { get; set; }

    public decimal? TravelTotal { get; set; }

    public decimal? SubTotal { get; set; }

    public decimal? TaxAmount { get; set; }

    public decimal? InvoiceTotal { get; set; }

    public decimal? PhoneSolveTotal { get; set; }

    public decimal? ProjectTotal { get; set; }

    public string? OverTimeRequestApproved { get; set; }

    public DateTime? DateCheckCreated { get; set; }

    public decimal? AmountPaid { get; set; }

    public decimal? AdjustmentAmount { get; set; }

    public string? ApprovedBy { get; set; }
}
