using System.Web;
using System.Web.UI;

namespace FarmerBrothers.Utilities
{
    public class IeCompatibilityModeDisabler : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.PreSendRequestHeaders += (sender, e) => DisableCompatibilityModeIfApplicable();
        }

        private void DisableCompatibilityModeIfApplicable()
        {
            if (IsIe && IsPage)
                DisableCompatibilityMode();
        }

        private void DisableCompatibilityMode()
        {
            var response = Context.Response;
            response.AddHeader("X-UA-Compatible", "IE=edge");
        }

        private bool IsIe { get { return Context.Request.Browser.IsBrowser("IE"); } }

        private bool IsPage { get { return Context.Handler is Page; } }

        private HttpContext Context { get { return HttpContext.Current; } }

        public void Dispose() { }
    }
}