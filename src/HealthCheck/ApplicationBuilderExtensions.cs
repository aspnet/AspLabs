using HealthChecks;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseHealthChecks(this IApplicationBuilder builder, string path)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.UseMiddleware<HealthCheckMiddleware>(path);
        }

        public static IApplicationBuilder UseHealthChecks(this IApplicationBuilder builder, string path, int port)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.UseMiddleware<HealthCheckMiddleware>(path, port);
        }
    }
}
