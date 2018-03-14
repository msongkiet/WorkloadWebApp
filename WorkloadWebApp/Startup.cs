using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WorkloadWebApp.Startup))]
namespace WorkloadWebApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
