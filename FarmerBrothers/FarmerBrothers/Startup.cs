using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(FarmerBrothers.Startup))]
namespace FarmerBrothers
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        }
    }
}
