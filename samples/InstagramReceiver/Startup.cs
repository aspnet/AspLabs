using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(InstagramReceiver.Startup))]
namespace InstagramReceiver
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
