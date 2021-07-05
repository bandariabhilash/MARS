using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FarmerBrothers.Data;
using System.ComponentModel.DataAnnotations;

namespace FarmerBrothers.Models
{
    public class InvoiceSearchModel
    {
        public List<FSM_View> FSMList { get; set; }
        public IList<InvoiceSearchResults> SearchResults;
        public DisplayInvoiceModel displayInvoiceModel;
        public List<string> InvoiceStatus = Utilities.Utility.InvoiceStatus;
        public int InvoiceUniqueid { get; set; }
        public string Invoiceid { get; set; }
        public double? WorkorderID { get; set; }
        public double? CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string SelectedInvoiceStatus { get; set; }
        public DateTime? BeforeDate { get; set; }
        public DateTime? OnOrAfterDate { get; set; }
        public int? FSM { get; set; }
        public double? TPSPVendorID { get; set; }
        public string TPSPVendorName { get; set; }
        public double? BranchID { get; set; }
        public string BranchName { get; set; }
        public float floatLocationID { get; set; }
        public string strLocationName { get; set; }
    }

    public class InvoiceSearchResults
    {
        public int InvoiceUniqueid { get; set; }
        public string Invoiceid { get; set; }
        public Nullable<int> WorkorderID { get; set; }
        public Nullable<decimal> SubmitAmount { get; set; }
        public Nullable<DateTime> InvoiceSubmitDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        public Nullable<decimal> AuthorizedAmount { set; get; }
        public string ApproveBy { set; get; }
        public Nullable<DateTime> DateSubmissionForPayment { set; get; }
        public string InvoiceStatus { set; get; }
        public string FSM { set; get; }
        public double? BranchID { set; get; }
        public string BranchName { set; get; }
        public string BranchState { set; get; }
        public int? CustomerID { set; get; }
        public string CustomerName { set; get; }
        public Nullable<DateTime> WorkorderCompletionDate { get; set; }
        public Nullable<DateTime> PaymentDate { get; set; }
        public string CheckNumber { set; get; }
        public double? TPSPVendorID { get; set; }
        public double? FSMID { get; set; }
        public int? ResponsibleTechBranch { get; set; }
        public string TPSPVendorName { get; set; }
        public string InvoiceSubmitDateString { set; get; }
        public string DateSubmissionForPaymentString { set; get; }
        public string WorkorderCompletionDateString { set; get; }
        public string PaymentDateString { set; get; }
        public double? TechID { set; get; }
    }

    public class DisplayInvoiceModel
    {
        public int InvoiceUniqueid { get; set; }
        public string Invoiceid { get; set; }
        public Nullable<int> WorkorderID { get; set; }
        public Nullable<decimal> SubmitAmount { get; set; }

        public Nullable<decimal> AdjustmentAmount { get; set; }

        public Nullable<System.DateTime> InvoiceSubmitDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        public Nullable<decimal> AuthorizedAmount { set; get; }
        public Nullable<decimal> AdditionalCharge { set; get; }
        public string ApproveBy { set; get; }
        public DateTime DateSubmissionForPayment { set; get; }
        public string InvoiceStatus { set; get; }
        public string FSM { set; get; }
        public int BranchId { set; get; }
        public string BranchName { set; get; }
        public string BranchState { set; get; }
        public int? CustomerID { set; get; }
        public int? PhoneSolveID { set; get; }
        public string CustomerName { set; get; }
        public string CustomerState { set; get; }
        public Nullable<System.DateTime> WorkorderCompletionDate { get; set; }
        public Nullable<System.DateTime> PaymentDate { get; set; }
        public string CheckNumber { set; get; }
        public int? TPSPVendorID { get; set; }
        public string TPSPVendorName { get; set; }
        public string Comments { get; set; }
        public string AdditionalChargeDescription { get; set; }
        public string InvoiceComments { get; set; }
        [DisplayFormat(DataFormatString = "{0:n2}")]
        public Nullable<decimal> PartsTotal { get; set; }
        [DisplayFormat(DataFormatString = "{0:n2}")]
        public Nullable<decimal> LaborTotal { get; set; }
        [DisplayFormat(DataFormatString = "{0:n2}")]
        public Nullable<decimal> TravelTotal { get; set; }
        [DisplayFormat(DataFormatString = "{0:n2}")]
        public Nullable<decimal> SubTotal { get; set; }
        [DisplayFormat(DataFormatString = "{0:n2}")]
        public Nullable<decimal> TaxAmount { get; set; }
        [DisplayFormat(DataFormatString = "{0:n2}")]
        public Nullable<decimal> InvoiceTotal { get; set; }
        [DisplayFormat(DataFormatString = "{0:n2}")]
        public Nullable<decimal> MileageTotal { get; set; }
        public Nullable<decimal> Mileage { get; set; }
        public double? TechID { set; get; }
        //public string flatRate { get; set; }

    }

    public class FSM_View
    {
        public int? FsmID { get; set; }
        public string FsmName { get; set; }
    }

    public class Invoice_Feast_Data
    {
        public double? Tech_Id { get; set; }
        public string Tech_Name { get; set; }
        public string Tech_Type { get; set; }
        public string Tech_Desc { get; set; }
        public string Tech_Phone { get; set; }
        public string tech_email { get; set; }
        public double ServiceCenter_Id { get; set; }
        public string ServiceCenter_Name { get; set; }
        public string ServiceCenter_Type { get; set; }
        public string ServiceCenter_Desc { get; set; }
        public string ServiceCenter_Phone { get; set; }
        public string ServiceCenter_zip { get; set; }
        public string Type { get; set; }
        public double? TeamLead_Id { get; set; }
        public string TeamLead_Name { get; set; }
        public string TeamLead_Type { get; set; }
        public string TeamLead_Desc { get; set; }
        public double? FSM_Id { get; set; }
        public string FSM_Name { get; set; }
        public string FSM_Type { get; set; }
        public string FSM_Desc { get; set; }
        public string DEFAULT_SERVICE_CENTER { get; set; }

        public string Tech_State { get; set; }

    }
}
