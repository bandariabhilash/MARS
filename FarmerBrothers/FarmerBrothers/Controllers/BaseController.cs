using FarmerBrothers.Data;
using Syncfusion.JavaScript.Models;
using Syncfusion.JavaScript.Shared.Serializer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;


namespace FarmerBrothers.Controllers
{
    public class DMSerial : IDataSourceSerializer
    {
        public string Serialize(object obj)
        {
            var str = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            str = str.Replace("'", "&#39;");
            return str;
        }
    }

    [SessionTimeout]
    [AuthorizeFB]    
    public class BaseController : Controller
    {
       
        protected FarmerBrothersEntities FarmerBrothersEntitites;

        private string userName;
        protected string UserName
        {
            get
            {
                if (string.IsNullOrEmpty(userName))
                {
                    if (Session != null)
                    {
                        if (Session["Username"] != null)
                        {
                            userName = Session["Username"].ToString();
                        }
                        else
                        {
                            Session.Clear();
                            FormsAuthentication.SignOut();
                            Session.Abandon();
                            RedirectToAction("Login", "Home");
                        }
                    }
                }

                return userName;
            }
        }

        public BaseController()
        {
            FarmerBrothersEntitites = new Data.FarmerBrothersEntities();           
            DataManagerConverter.Serializer = new DMSerial();            
        }

        private void CheckSessionTimeout()
        {
            string msgSession = "Warning: Within next 3 minutes, if you do not do anything, " +
                       " our system will redirect to the login page. Please save changed data.";
            //time to remind, 3 minutes before session ends
            int int_MilliSecondsTimeReminder = (this.Session.Timeout * 60000) - 3 * 60000;
            //time to redirect, 5 milliseconds before session ends
            int int_MilliSecondsTimeOut = (this.Session.Timeout * 60000) - 5;

            string str_Script = @"
            var myTimeReminder, myTimeOut; 
            clearTimeout(myTimeReminder); 
            clearTimeout(myTimeOut); " +
                    "var sessionTimeReminder = " +
                int_MilliSecondsTimeReminder.ToString() + "; " +
                    "var sessionTimeout = " + int_MilliSecondsTimeOut.ToString() + ";" +
                    "function doReminder(){ }" +
                    "function doRedirect(){ window.location.href='../../TimeOut.asp'; }" + @"
            myTimeReminder=setTimeout('doReminder()', sessionTimeReminder); 
            myTimeOut=setTimeout('doRedirect()', sessionTimeout); ";

            //ScriptManager.RegisterClientScriptBlock(this.Page, this.GetType(),
            //      "CheckSessionOut", str_Script, true);
        }

        protected GridProperties ConvertGridObject(string gridProperty)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            IEnumerable div = (IEnumerable)serializer.Deserialize(gridProperty, typeof(IEnumerable));
            GridProperties gridProp = new GridProperties();
            foreach (KeyValuePair<string, object> ds in div)
            {
                var property = gridProp.GetType()
                    .GetProperty(ds.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (property != null)
                {
                    Type type = property.PropertyType;
                    string serialize = serializer.Serialize(ds.Value);
                    object value = serializer.Deserialize(serialize, type);
                    property.SetValue(gridProp, value, null);
                }
            }
            return gridProp;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (FarmerBrothersEntitites != null)
                {
                    FarmerBrothersEntitites.Dispose();
                }
            }

            base.Dispose(disposing);
        }
      
    }
}