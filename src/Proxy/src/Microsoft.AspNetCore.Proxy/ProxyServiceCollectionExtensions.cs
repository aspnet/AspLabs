// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Proxy;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ProxyServiceCollectionExtensions
    {
        public static IServiceCollection AddProxy(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services.AddSingleton<ProxyService>();
        }

        public static IServiceCollection AddProxy(this IServiceCollection services, Action<SharedProxyOptions> configureOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            services.Configure(configureOptions);
            return services.AddSingleton<ProxyService>();
        }
    }
}
