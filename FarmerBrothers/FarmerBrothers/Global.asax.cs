using FarmerBrothers.Models;
using FarmerBrothers.Utilities;
using System;
using System.Data.Common;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Xml;
using FarmerBrothers.Data;
using System.Data.Entity.Validation;

namespace FarmerBrothers
{
    public class Global : System.Web.HttpApplication
    {
        #region Constants

        public const string LOG_ERROR_IN = "Error in: ";
        public const string LOG_ERROR_MESSAGE = "Error Message: ";
        public const string LOG_STACK_TRACE = "Stack Trace: ";
        public const string LOG_SOURCE = "Source: ";
        public const string LOG_TARGET_SITE = "Target Site: ";
        public const string LOG_USER_UID = "User Uid: ";

        #endregion

        private static IeCompatibilityModeDisabler module;

        protected void Application_Start()
        {
            module = new IeCompatibilityModeDisabler();

            HttpConfiguration config = GlobalConfiguration.Configuration;

            config.Formatters.JsonFormatter
                        .SerializerSettings
                        .ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            ModelBinders.Binders.Add(typeof(WorkorderManagementModel), new WorkorderManagementModelBinder());
            ModelBinders.Binders.Add(typeof(NewProfile), new NewProfileBinder());
            ModelBinders.Binders.Add(typeof(ErfModel), new ErfModelBinder());
            ModelBinders.Binders.Add(typeof(CallCloserModel), new CallCloserModelBinder());



        }



        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            //if (Request.Url.AbsoluteUri.Contains("encrypt"))
            //{
            //    Context.Response.Redirect(Utility.DecryptUrl(Request.Url.AbsoluteUri));
            //}
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();
        }

        protected void Application_Error()
        {

            StringBuilder message = new StringBuilder();
            Exception exception;
            exception = Server.GetLastError();
            if (exception is DbEntityValidationException)
            {
                DbEntityValidationException realDBException = exception as DbEntityValidationException;
                string errorDetails = string.Empty;
                foreach (var eve in realDBException.EntityValidationErrors)
                {
                    errorDetails += string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:", eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    errorDetails += Environment.NewLine;
                    foreach (var ve in eve.ValidationErrors)
                    {
                        errorDetails += string.Format("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage);
                        errorDetails += Environment.NewLine;
                    }
                }
                LogError(errorDetails);
            }
            else
            {
                if (exception is HttpUnhandledException && exception.InnerException != null)
                {
                    exception = exception.InnerException;
                }
                while (exception != null)
                {
                    message.Append(FormatException(exception));
                    exception = exception.InnerException;
                }
                LogError(message.ToString());

            }

            HttpContext httpContext = HttpContext.Current;
            if (httpContext != null)
            {
                RequestContext requestContext = ((MvcHandler)httpContext.CurrentHandler).RequestContext;
                /* when the request is ajax the system can automatically handle a mistake with a JSON response. then overwrites the default response */
                if (requestContext.HttpContext.Request.IsAjaxRequest())
                {
                    try
                    {
                        httpContext.Response.Clear();
                        string controllerName = requestContext.RouteData.GetRequiredString("controller");
                        IControllerFactory factory = ControllerBuilder.Current.GetControllerFactory();
                        IController controller = factory.CreateController(requestContext, controllerName);
                        ControllerContext controllerContext = new ControllerContext(requestContext, (ControllerBase)controller);

                        JsonResult jsonResult = new JsonResult();
                        jsonResult.Data = new { success = false, serverError = "500" };
                        jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                        jsonResult.ExecuteResult(controllerContext);
                        httpContext.Response.End();
                    }
                    catch (Exception ex)
                    {

                    }
                }
                else
                {
                    httpContext.Response.Redirect("~/Error");
                }
            }
        }

        public override void Init()
        {
            base.Init();
            module.Init(this);
        }
        private string FormatException(Exception ex)
        {
            DbException dbEx = ex as DbException;
            string message = String.Empty;
            if (dbEx == null)
            {
                message =
                    LOG_ERROR_IN + Request.Url + Environment.NewLine +
                    LOG_ERROR_MESSAGE + ex.Message + Environment.NewLine +
                    LOG_STACK_TRACE + ex.StackTrace + Environment.NewLine +
                    LOG_SOURCE + ex.Source + Environment.NewLine +
                    LOG_TARGET_SITE + ex.TargetSite + Environment.NewLine +
                    LOG_USER_UID + this.Context.User.Identity.Name + Environment.NewLine + Environment.NewLine;
            }
            else
            {
                message =
                    LOG_ERROR_IN + Request.Url + Environment.NewLine +
                    LOG_ERROR_MESSAGE + dbEx.Message + Environment.NewLine +
                    LOG_STACK_TRACE + dbEx.StackTrace + Environment.NewLine +
                    LOG_SOURCE + dbEx.Source + Environment.NewLine +
                    LOG_TARGET_SITE + dbEx.TargetSite + Environment.NewLine +
                    LOG_USER_UID + this.Context.User.Identity.Name + Environment.NewLine + Environment.NewLine;
            }
            return message;
        }

        public static void LogError(string Message)
        {
            if (!Message.Contains("arterySignalR/ping"))
            {
                using (FarmerBrothersEntities entity = new FarmerBrothersEntities())
                {
                    FBActivityLog log = new FBActivityLog();
                    log.LogDate = DateTime.UtcNow;
                    log.UserId = System.Web.HttpContext.Current.Session["UserId"] != null ? (int)System.Web.HttpContext.Current.Session["UserId"] : 1;
                    log.ErrorDetails = Message;
                    entity.FBActivityLogs.Add(log);
                    entity.SaveChanges();
                }
            }

        }
    }
}