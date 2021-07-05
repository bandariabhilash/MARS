using FarmerBrothers.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FarmerBrothers
{
    public class AuthorizeFBAttribute : AuthorizeAttribute
    {
        private bool _authenticated;
        private bool _authorized;

        public string ControllerName { get; set; }
        public string ActionName { get; set; }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            base.HandleUnauthorizedRequest(filterContext);

            //TODO :: LG :: _authorized need to handle authorization
            if (_authenticated && _authorized)            
            {
                if (System.Web.HttpContext.Current.Session["UserRoleId"] == null)
                {
                    filterContext.Result = new RedirectResult("~/Home/Login");
                }
                else
                {
                    filterContext.Result = new RedirectResult("~/Home/Error");
                }
            }
            else
            {
                filterContext.Result = new RedirectResult("~/Home/Login");
            }
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            
            if (System.Web.HttpContext.Current.Session["Username"] == null)
            {
                ControllerName = "Home";
                ActionName = "Login";
                _authenticated = false;
            }
            else
            {
                _authenticated = HttpContext.Current.User.Identity.IsAuthenticated;
                ControllerName = httpContext.Request.RequestContext.RouteData.Values["controller"].ToString();
                ActionName = httpContext.Request.RequestContext.RouteData.Values["action"].ToString();
            }
            
            //NameValueCollection section = (NameValueCollection)ConfigurationManager.GetSection("LdapRoles");
            if (_authenticated)
            {
                //var groups = section[ControllerName].Split(',');
                string username = httpContext.User.Identity.Name;
                try
                {
                    AuthenticationHelper objLdapAuthentication = new AuthenticationHelper();
                    _authorized = true;//objLdapAuthentication.UserIsMemberOfGroups(username, groups);
                    return _authorized;
                }
                catch (Exception ex)
                {
                    //Log the exception.
                    //this.Log().Error(() => "Error attempting to authorize user", ex);
                    _authorized = false;
                    return _authorized;
                }
            }

            _authorized = false;
            return _authorized;
        }
    }
}