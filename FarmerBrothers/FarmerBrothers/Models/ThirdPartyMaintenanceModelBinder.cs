using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FarmerBrothers.Models
{
    public class ThirdPartyMaintenanceModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext,
                              ModelBindingContext bindingContext)
        {
            HttpRequestBase request = controllerContext.HttpContext.Request;

            ThirdPartyMaintenanceModel model = new ThirdPartyMaintenanceModel();
           
           
            foreach (var property in model.GetType().GetProperties())
            {

                if (property.Name == "RatePerPallet" || property.Name == "TravelRatePerMile" || property.Name == "TravelHourlyRate" || property.Name == "TravelOvertimeRate" || property.Name == "LaborHourlyRate" || property.Name == "LaborOvertimeRate")
                {

                    var value = request.Form.Get(property.Name);

                    if (value != null)
                    {
                        decimal amount;
                        if (Decimal.TryParse(value, NumberStyles.Currency, null, out amount))

                            property.SetValue(model, amount);
                        bindingContext.ModelState.AddModelError("Amount", "Wrong amount format");
                    }


                }

                else
                {
                    if (property.PropertyType == typeof(Int32))
                    {
                        property.SetValue(model, Convert.ToInt32(request.Form.Get(property.Name)));
                    }
                    else if (property.PropertyType == typeof(Nullable<int>))
                    {
                        property.SetValue(model, new Nullable<int>(Convert.ToInt32(request.Form.Get(property.Name))));
                    }
                    else if(
                      property.PropertyType == typeof(Nullable<decimal>))
                    {
                        property.SetValue(model, Convert.ToDecimal(request.Form.Get(property.Name)));
                    }
                    else if (property.PropertyType == typeof(bool))
                    {
                        if ((request.Form.Get(property.Name)) == "false")
                        {
                            property.SetValue(model, false);
                        }
                        else
                        {
                            property.SetValue(model, true);
                        }
                    }
                    else
                    {
                        property.SetValue(model, (request.Form.Get(property.Name)));
                    }
                   
                }
                 

            }
            return model;
        }
    }
}
