using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CustomSender.Startup))]
namespace CustomSender
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
