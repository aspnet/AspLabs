using System.Web.Http;

namespace CustomSender
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

            // Load basic support for sending WebHooks
            config.InitializeCustomWebHooks();

            // Load Azure Storage or SQL for persisting subscriptions
            config.InitializeCustomWebHooksAzureStorage();
            // config.InitializeCustomWebHooksSqlStorage();

            // Load Web API controllers for managing subscriptions
            config.InitializeCustomWebHooksApis();
        }
    }
}
