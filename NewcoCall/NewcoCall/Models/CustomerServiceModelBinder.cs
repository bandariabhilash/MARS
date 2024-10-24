using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace NewcoCall.Models
{
    public class CustomerServiceModelBinder : IModelBinder
    {

        public object BindModel(ControllerContext controllerContext,
                                ModelBindingContext bindingContext)
        {
            HttpRequestBase request = controllerContext.HttpContext.Request;

            CustomerServiceModel model = new CustomerServiceModel();

            JavaScriptSerializer json_serializer = new JavaScriptSerializer();


            model.AccountNumber = Convert.ToInt32(request.Unvalidated.Form.Get("AccountNumber"));
            model.CallReason = request.Unvalidated.Form.Get("callReason");

            model.CustomerName = request.Unvalidated.Form.Get("CustomerName");
            model.Address1 = request.Unvalidated.Form.Get("Address1");
            model.Address2 = request.Unvalidated.Form.Get("Address2");
            model.City = request.Unvalidated.Form.Get("City");
            model.State = request.Unvalidated.Form.Get("State");
            model.PostalCode = request.Unvalidated.Form.Get("PostalCode");
            model.MainContactName = request.Unvalidated.Form.Get("MainContactName");
            model.PhoneNumber = request.Unvalidated.Form.Get("PhoneNumber");
            model.Email = request.Unvalidated.Form.Get("Email");
            model.Comments = request.Unvalidated.Form.Get("Comments");
            model.PaymentTransactionId = request.Unvalidated.Form.Get("PaymentTransactionId");
            model.PaymentTerm = request.Unvalidated.Form.Get("PaymentTerm");
            model.ServiceType = request.Unvalidated.Form.Get("ServiceType");

            return model;
        }
        

    }
}