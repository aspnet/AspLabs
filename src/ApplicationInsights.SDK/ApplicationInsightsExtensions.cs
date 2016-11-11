using System;
using System.Collections.Generic;
using ApplicationInsights.AspNetCore;
using ApplicationInsights.AspNetCore.TelemetryInitializers;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Hosting
{
    public static class ApplicationInsightsExtensions
    {
        private const string InstrumentationKeyFromConfig = "ApplicationInsights:InstrumentationKey";
        private const string DeveloperModeFromConfig = "ApplicationInsights:TelemetryChannel:DeveloperMode";
        private const string EndpointAddressFromConfig = "ApplicationInsights:TelemetryChannel:EndpointAddress";

        private const string InstrumentationKeyForWebSites = "APPINSIGHTS_INSTRUMENTATIONKEY";
        private const string DeveloperModeForWebSites = "APPINSIGHTS_DEVELOPER_MODE";
        private const string EndpointAddressForWebSites = "APPINSIGHTS_ENDPOINTADDRESS";
        public static IWebHostBuilder UseApplicationInsights(this IWebHostBuilder hostBuilder)
        {
            if (hostBuilder == null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }

            return hostBuilder.ConfigureServices(services =>
            {
                services.AddApplicationInsightsTelemetry();
            });
        }

        public static IServiceCollection AddApplicationInsightsTelemetry(this IServiceCollection services)
        {
            // Original code, but I'm not sure if we need it still
            //services.AddScoped<RequestTelemetry>((svcs) =>
            //{
            //    // Default constructor need to be used
            //    return new RequestTelemetry();
            //});
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<ApplicationInitializer>();
            services.AddSingleton<TelemetryClient>(serviceProvider =>
            {
                return new TelemetryClient(serviceProvider.GetRequiredService<IOptions<TelemetryConfiguration>>().Value);
            });
            services.AddSingleton<ApplicationInsightsJavascriptService>();
            services.AddSingleton<IStartupFilter>(new ApplicationInsightsStartupFilter());
            services.AddSingleton<IOptions<TelemetryConfiguration>, TelemetryConfigurationOptions>();
            services.AddSingleton<IConfigureOptions<TelemetryConfiguration>, ApplicationInsightsSetup>();
            //services.AddSingleton<ITelemetryInitializer, ClientIpHeaderTelemetryInitializer>();

            return services;
        }

        public static IServiceCollection ConfigureApplicationInsightsTelemetry(this IServiceCollection services, IConfiguration configuration)
        {
            return services.Configure<TelemetryConfiguration>(o =>
            {
                AddTelemetryConfiguration(configuration, o);
            });
        }

        private static void AddTelemetryConfiguration(IConfiguration config, TelemetryConfiguration telemetryConfiguration)
        {
            var iKey = Environment.GetEnvironmentVariable(InstrumentationKeyForWebSites);
            if (!string.IsNullOrWhiteSpace(iKey))
            {
                telemetryConfiguration.InstrumentationKey = iKey;
            }

            if (config != null)
            {
                string instrumentationKey = config[InstrumentationKeyForWebSites];
                if (string.IsNullOrWhiteSpace(instrumentationKey))
                {
                    instrumentationKey = config[InstrumentationKeyFromConfig];
                }

                if (!string.IsNullOrWhiteSpace(instrumentationKey))
                {
                    telemetryConfiguration.InstrumentationKey = instrumentationKey;
                }

                string developerModeValue = config[DeveloperModeForWebSites];
                if (string.IsNullOrWhiteSpace(developerModeValue))
                {
                    developerModeValue = config[DeveloperModeFromConfig];
                }

                if (!string.IsNullOrWhiteSpace(developerModeValue))
                {
                    bool developerMode = false;
                    if (bool.TryParse(developerModeValue, out developerMode))
                    {
                        telemetryConfiguration.TelemetryChannel.DeveloperMode = developerMode;
                    }
                }

                string endpointAddress = config[EndpointAddressForWebSites];
                if (string.IsNullOrWhiteSpace(endpointAddress))
                {
                    endpointAddress = config[EndpointAddressFromConfig];
                }

                if (!string.IsNullOrWhiteSpace(endpointAddress))
                {
                    telemetryConfiguration.TelemetryChannel.EndpointAddress = endpointAddress;
                }
            }
        }

        public class TelemetryConfigurationOptions : IOptions<TelemetryConfiguration>
        {
            public TelemetryConfigurationOptions(IEnumerable<IConfigureOptions<TelemetryConfiguration>> configs)
            {
                foreach (var c in configs)
                {
                    c.Configure(Value);
                }
            }

            public TelemetryConfiguration Value => TelemetryConfiguration.Active;
        }

        public class ApplicationInsightsSetup : IConfigureOptions<TelemetryConfiguration>
        {
            private readonly IEnumerable<ITelemetryInitializer> _initializers;
            private readonly IEnumerable<ITelemetryModule> _modules;

            public ApplicationInsightsSetup(IEnumerable<ITelemetryInitializer> initializers,
                                            IEnumerable<ITelemetryModule> modules)
            {
                _initializers = initializers;
                _modules = modules;
            }

            public void Configure(TelemetryConfiguration options)
            {
                foreach (var initializer in _initializers)
                {
                    options.TelemetryInitializers.Add(initializer);
                }

                foreach (var module in _modules)
                {
                    module.Initialize(options);
                }
            }
        }
    }
}
