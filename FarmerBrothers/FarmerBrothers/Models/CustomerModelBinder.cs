using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FarmerBrothers.Models
{
    public class CustomerModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            HttpRequestBase request = controllerContext.HttpContext.Request;

            CustomerModel model = new CustomerModel();
            foreach (var property in model.GetType().GetProperties())
            {
                property.SetValue(model, request.Unvalidated.Form.Get(property.Name));
            }

            //if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("Customer.Zipcode")))
            //{
            //    model.ZipCode = request.Unvalidated.Form.Get("Customer.Zipcode");
            //}
            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("Zipcode")))
            {
                model.ZipCode = request.Unvalidated.Form.Get("Zipcode");
            }
            return model;
        }
    }
}