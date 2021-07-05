using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace FarmerBrothers.Models
{
    public class NewProfileBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext,
                                ModelBindingContext bindingContext)
        {
            HttpRequestBase request = controllerContext.HttpContext.Request;

            JavaScriptSerializer json_serializer = new JavaScriptSerializer();

           

            NewProfile newprofile = new NewProfile();

            foreach (var property in newprofile.GetType().GetProperties())
            {
                string value = request.Unvalidated.Form.Get(property.Name);
                if (!string.IsNullOrWhiteSpace(value) && property.PropertyType == typeof(Int32))
                {
                    property.SetValue(newprofile, Convert.ToInt32(value));
                }
                else if (!string.IsNullOrWhiteSpace(value))
                {
                    if (property.Name == "Operation")
                    {
                        newprofile.Operation = (WorkOrderManagementSubmitType)Convert.ToInt32(request.Unvalidated.Form.Get("Operation"));
                    }
                    else if(property.Name == "IsPrimaryTechnician")
                    {
                        string isPrimaryTechCheckValue = "false";
                        if (request.Unvalidated.Form.Get("IsPrimaryTechnician") != null)
                            isPrimaryTechCheckValue = (request.Unvalidated.Form.Get("IsPrimaryTechnician").Split(','))[0];
                        if (isPrimaryTechCheckValue.Contains("true"))
                            newprofile.IsPrimaryTechnician = true;
                        else
                            newprofile.IsPrimaryTechnician = false;
                    }
                    //else if (property.Name == "IsTechUser")
                    //{
                    //    string userType = request.Unvalidated.Form.Get("IsTechUser").ToString();

                    //    if (userType.ToLower() == "erf")
                    //    {
                    //        newprofile.IsERFUser = true;
                    //        newprofile.IsTechnician = false;
                    //    }
                    //    else if (userType.ToLower() == "tech")
                    //    {
                    //        newprofile.IsERFUser = false;
                    //        newprofile.IsTechnician = true;
                    //    }
                    //    else
                    //    {
                    //        newprofile.IsERFUser = false;
                    //        newprofile.IsTechnician = false;
                    //    }
                    //}
                    else
                    {
                        property.SetValue(newprofile, request.Form.Get(property.Name));
                    }
                    
                }
                
            }

            newprofile.modulePrivilegeModel = new UserModulePrivilegeModel();
            newprofile.modulePrivilegeModel.AppRoleModel = new List<ApplicationRoleModel>();
            newprofile.modulePrivilegeModel.Modules = new ApplicationModel();
            newprofile.modulePrivilegeModel.Privileges = new List<PrivilegeModel>();
            if (!string.IsNullOrWhiteSpace(request.Unvalidated.Form.Get("ApplicationPrivilegeHidden")))
            {
                newprofile.modulePrivilegeModel.AppRoleModel = json_serializer.Deserialize<IList<ApplicationRoleModel>>(request.Unvalidated.Form.Get("ApplicationPrivilegeHidden"));
            }
            return newprofile;
        }
    }
}