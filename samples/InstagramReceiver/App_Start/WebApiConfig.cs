using System.Web.Http;

namespace InstagramReceiver
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Wire up dependencies
            Dependencies.Initialize(config);

            // Initialize Instagram WebHook receiver
            config.InitializeReceiveInstagramWebHooks();
        }
    }
}
