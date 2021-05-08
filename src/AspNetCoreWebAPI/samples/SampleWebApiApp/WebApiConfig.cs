using System.Net.Http.Formatting;
using System.Web.Http;
using SampleWebApiApp.Handlers;

namespace SampleWebApiApp
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Optionally make System.Text.Json the preferred formatter
            // config.Formatters.Insert(0, new SystemTextJsonMediaTypeFormatter());

            config.MessageHandlers.Add(new AddResponseHeaderHandler());

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
