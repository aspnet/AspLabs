using System;
using ApplicationInsights.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Hosting
{
    internal class ApplicationInsightsStartupFilter: IStartupFilter
    {
        public ApplicationInsightsStartupFilter()
        {
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                var initializer = app.ApplicationServices.GetRequiredService<ApplicationInitializer>();
                initializer.Initialize();
                next(app);
            };
        }

    }
}