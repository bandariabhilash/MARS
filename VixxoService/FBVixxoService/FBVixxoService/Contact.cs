//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FBVixxoService
{
    using System;
    using System.Collections.Generic;
    
    public partial class Contact
    {
        public int ContactID { get; set; }
        public string CompanyName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Phone { get; set; }
        public string Division { get; set; }
        public string DivisionDesc { get; set; }
        public string District { get; set; }
        public string DistrictDesc { get; set; }
        public string FamilyAff { get; set; }
        public string FamilyAffDesc { get; set; }
        public string PricingParent { get; set; }
        public string PricingParentName { get; set; }
        public string AcctRep { get; set; }
        public string AcctRepDesc { get; set; }
        public Nullable<int> RouteNumber { get; set; }
        public Nullable<int> Distributor { get; set; }
        public string DistributorName { get; set; }
        public string DeliveryMethod { get; set; }
        public string DeliveryDesc { get; set; }
        public string SearchType { get; set; }
        public string SearchDesc { get; set; }
        public string BusinessUnit { get; set; }
        public Nullable<System.DateTime> DateCreated { get; set; }
        public Nullable<System.DateTime> LastModified { get; set; }
        public string AreaCode { get; set; }
        public Nullable<short> ContactActive { get; set; }
        public string LongAddressNumber { get; set; }
        public Nullable<System.DateTime> LastModifiedByFTP { get; set; }
        public Nullable<int> AuditFlow { get; set; }
        public Nullable<int> SendInvoiceTo { get; set; }
        public Nullable<int> FSMJDE { get; set; }
        public string FieldServiceManager { get; set; }
        public string ServiceLevelCode { get; set; }
        public Nullable<System.DateTime> AnniversaryDate { get; set; }
        public string TierDesc { get; set; }
        public Nullable<int> RemainingFreeCalls { get; set; }
        public Nullable<int> CCMJDE { get; set; }
        public Nullable<int> RCCMJDE { get; set; }
        public string CategoryCode { get; set; }
        public string OperatingUnit { get; set; }
        public string Route { get; set; }
        public string Branch { get; set; }
        public string C1099Reporting { get; set; }
        public string Chain { get; set; }
        public string BrewmaticAgentCode { get; set; }
        public string NTR { get; set; }
        public string DelDayOrFOB { get; set; }
        public string RouteCode { get; set; }
        public string ZoneNumber { get; set; }
        public string MailingName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public Nullable<int> FBProviderID { get; set; }
        public Nullable<int> NOFBPSPEmails { get; set; }
        public Nullable<System.DateTime> LastAutoEmail { get; set; }
        public string CustomerRegion { get; set; }
        public string RegionNumber { get; set; }
        public string CustomerBranch { get; set; }
        public string PricingParentID { get; set; }
        public string PricingParentDesc { get; set; }
        public string LastSaleDate { get; set; }
        public string Email { get; set; }
        public string CustomerSpecialInstructions { get; set; }
        public string PhoneWithAreaCode { get; set; }
        public Nullable<int> IsUnknownUser { get; set; }
        public string BillingCode { get; set; }
        public Nullable<bool> FilterReplaced { get; set; }
        public Nullable<System.DateTime> FilterReplacedDate { get; set; }
        public Nullable<System.DateTime> NextFilterReplacementDate { get; set; }
        public string SalesEmail { get; set; }
        public string ESMName { get; set; }
        public string ESMPhone { get; set; }
        public string ESMEmail { get; set; }
        public string RSMName { get; set; }
        public string RSMPhone { get; set; }
        public string RSMEmail { get; set; }
        public string CCMName { get; set; }
        public string CCMPhone { get; set; }
        public string CCMEmail { get; set; }
        public string DaysSinceLastSale { get; set; }
        public string ProfitabilityTier { get; set; }
        public string ContributionMargin { get; set; }
        public Nullable<decimal> NetSalesAmount { get; set; }
        public string PaymentTerm { get; set; }
        public Nullable<bool> IsNonFbCustomer { get; set; }
    }
}