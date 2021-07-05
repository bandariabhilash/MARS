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
    public class SessionTimeoutAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpContext ctx = HttpContext.Current;
            if (HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString().ToUpper() != "dispatchresponse".ToUpper()
                && HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString().ToUpper() != "CallClosure".ToUpper()
                && HttpContext.Current.Session["UserId"] == null)
            {
                filterContext.Result = new RedirectResult("~/Home/Login");
                return;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}