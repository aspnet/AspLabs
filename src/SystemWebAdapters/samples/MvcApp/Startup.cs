using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(MvcApp.Startup))]
namespace MvcApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
