using FarmerBrothers.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmerBrothers.Models
{
    public class BillingReportModel
    {
        public DateTime? BillingFromDate { get; set; }
        public DateTime? BillingToDate { get; set; }

        public int DealerId { get; set; }
        public string TechID { get; set; }
        public string ParentACC { get; set; }
        public DateTime? HiddenFromDate { get; set; }
        public DateTime? HiddenToDate { get; set; }
        public int HiddenDealerId { get; set; }
        public string HiddenTechID { get; set; }
        public string HiddenParentACC { get; set; }
        public string HiddenAccountNo { get; set; }
        public List<TECH_HIERARCHY> Technicianlist { get; set; }
        public List<Technician> FamilyAffs { get; set; }
        public string AccountNo { get; set; }

        public IList<BillingReportSearchResultModel> SearchResults;
    }

    public class BillingReportSearchResultModel
    {
        public string WorkorderID { get; set; }
        public string WorkorderEntryDate { get; set; }
        public string CustomerID { get; set; }
        public string CompanyName { get; set; }
        public string Address1 { get; set; }
        public string City { get; set; }
        public string Route { get; set; }
        public string Branch { get; set; }
        public string Techid { get; set; }
        public string TechName { get; set; }
        public string WorkorderCallstatus { get; set; }
        public string StartDateTime { get; set; }
        public string ArrivalDateTime { get; set; }
        public string CompletionDateTime { get; set; }
        public string PurchaseOrder { get; set; }
        public string BillingID { get; set; }
        public string ScheduleDate { get; set; }
        public string WorkorderEquipCount { get; set; }
        public string CustomerState { get; set; }
        public string ThirdPartyPO { get; set; }
        public string Estimate { get; set; }
        public string FinalEstimate { get; set; }
        public string EstimateApprovedBy { get; set; }
        public string OriginalWorkorderid { get; set; }
        public string WorkorderCalltypeid { get; set; }
        public string TechCalled { get; set; }
        public string AppointmentDate { get; set; }
        public string DispatchTechID { get; set; }
        public string DispatchTechName { get; set; }
        public string NoServiceRequired { get; set; }
        public string NSRReason { get; set; }
        public string PricingParentID { get; set; }
        public string Category { get; set; }
        public string SerialNumber { get; set; }
        public string Model { get; set; }
        public string Manufacturer { get; set; }
        public string Solutionid { get; set; }
        public string WorkPerformedNotes { get; set; }
        public string CustomerName { get; set; }
        public string Quantity { get; set; }
        public string Sku { get; set; }
        public decimal SKUCost { get; set; }
        public string VendorCode { get; set; }
        public string Description { get; set; }
        public string OrderSource { get; set; }
        public string Supplier { get; set; }
        public string TravelTotal { get; set; }
        public string LaborTotal { get; set; }
        public string PartsTotal { get; set; }
        public string TotalInvoice { get; set; }

    }
}