using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace FarmerBrothers.Models
{
    public class ProcessCardModel
    {
        public IList<FbWorkorderBillableSKUModel> PartsList;
        public IList<VendorDataModel> SKUList;

        public IList<CategoryModel> BillingItems;
        public IList<BillingModel> BillingDetails;

        public decimal BillingTotal;

        public decimal LaborCost;
        public decimal PreTravelCost;
        public decimal TravelCost;
        public decimal PartsCost;
        public decimal PartsDiscount;
        public decimal PartsDiscountCost;
        public decimal SalesCost;
        public decimal SaleTax;

        public string Name;
        public string CardNumber;
        public string ExpDate;
        public decimal Cvv;

        public string PaymentTransactionId;

        public string FinalTransactionId;

        public int WorkorderId;
        public int CustomerId;

        public DateTime? StartDateTime;
        public DateTime? ArrivalDateTime;
        public DateTime? CompletionDateTime;
        public DateTime? WorkorderEntryDate;

        public string FromView;
        public bool IsManualFeed;
        public string ReceiptEmail;
    }

    public class ProcessCardModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext,
                               ModelBindingContext bindingContext)
        {
            HttpRequestBase request = controllerContext.HttpContext.Request;
            ProcessCardModel model = new ProcessCardModel();
            JavaScriptSerializer json_serializer = new JavaScriptSerializer();


            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("BillingDetailsHidden")))
            {
                model.BillingDetails = json_serializer.Deserialize<IList<BillingModel>>(request.Unvalidated.Form.Get("BillingDetailsHidden"));
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("PartsDetailsHidden")))
            {
                model.PartsList = json_serializer.Deserialize<IList<FbWorkorderBillableSKUModel>>(request.Unvalidated.Form.Get("PartsDetailsHidden"));
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("laborTxtHidden")))
            {
                model.LaborCost = Convert.ToDecimal(request.Unvalidated.Form.Get("laborTxtHidden").ToString());
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("travelTxtHidden")))
            {
                model.TravelCost = Convert.ToDecimal(request.Unvalidated.Form.Get("travelTxtHidden").ToString());
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("partsTxtHidden")))
            {
                model.PartsCost = Convert.ToDecimal(request.Unvalidated.Form.Get("partsTxtHidden").ToString());
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("partsDiscountTxtHidden")))
            {
                model.PartsDiscountCost = Convert.ToDecimal(request.Unvalidated.Form.Get("partsDiscountTxtHidden").ToString().Replace("$", ""));
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("salesTxtHidden")))
            {
                model.SalesCost = Convert.ToDecimal(request.Unvalidated.Form.Get("salesTxtHidden").ToString().Replace("$", ""));
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("BillingTotalLblHidden")))
            {
                model.BillingTotal = Convert.ToDecimal(request.Unvalidated.Form.Get("BillingTotalLblHidden").ToString());
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("cloverTransactionIdHidden")))
            {
                model.PaymentTransactionId = request.Unvalidated.Form.Get("cloverTransactionIdHidden").ToString();
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("WorkorderIdHidden")))
            {
                model.WorkorderId = Convert.ToInt32(request.Unvalidated.Form.Get("WorkorderIdHidden"));
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("StartDateTimeHidden")))
            {
                model.StartDateTime = Convert.ToDateTime(request.Unvalidated.Form.Get("StartDateTimeHidden"));
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("ArrivalDateTimeHidden")))
            {
                model.ArrivalDateTime = Convert.ToDateTime(request.Unvalidated.Form.Get("ArrivalDateTimeHidden"));
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("CompletionDateTimeHidden")))
            {
                model.CompletionDateTime = Convert.ToDateTime(request.Unvalidated.Form.Get("CompletionDateTimeHidden"));
            }

            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("manualOverrideChkHidden")))
            {
                model.IsManualFeed = Convert.ToBoolean(request.Unvalidated.Form.Get("manualOverrideChkHidden"));
            }

            return model;
        }
    }


}