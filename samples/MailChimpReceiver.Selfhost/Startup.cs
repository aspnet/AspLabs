using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.WebHooks;
using Owin;

namespace MailChimpReceiver.Selfhost
{
    public class Startup
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();

            // Set the assembly resolver so that WebHooks receiver controller is loaded.
            WebHookAssemblyResolver assemblyResolver = new WebHookAssemblyResolver();
            config.Services.Replace(typeof(IAssembliesResolver), assemblyResolver);

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            appBuilder.UseWebApi(config);

            // Initialize MailChimp WebHooks
            config.InitializeReceiveMailChimpWebHooks();
        }
    }
}
