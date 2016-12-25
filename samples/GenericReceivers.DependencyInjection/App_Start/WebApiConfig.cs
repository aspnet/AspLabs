using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using GenericReceivers.Dependencies;
using GenericReceivers.WebHooks;
using log4net.Config;
using Microsoft.AspNet.WebHooks;
using Microsoft.AspNet.WebHooks.Diagnostics;

namespace GenericReceivers.DependencyInjection
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

            // Initialize log4net
            XmlConfigurator.Configure();

            // Initialize AutoFac container
            ContainerBuilder builder = new ContainerBuilder();

            // Register WebHook handlers
            builder.RegisterType<GenericJsonWebHookHandler>().As<IWebHookHandler>();

            // Register our custom dependencies used by our GenericJsonWebHookHandler
            builder.RegisterType<MyDependency>().As<IMyDependency>();

            // Register our log4net logger
            builder.RegisterType<Log4NetLogger>().As<ILogger>().SingleInstance();

            // Register our own receiver config provider instead of reading them from web.config
            builder.RegisterType<ReceiverConfig>().As<IWebHookReceiverConfig>().SingleInstance();

            // Register the container with Web API
            IContainer container = builder.Build();
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            // Initialize Slack WebHook receiver
            config.InitializeReceiveGenericJsonWebHooks();
        }
    }
}
