// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.WebHooks.ApplicationModels;
using Microsoft.AspNetCore.WebHooks.Filters;
using Microsoft.AspNetCore.WebHooks.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.WebHooks.Internal
{
    /// <summary>
    /// Methods to add services for WebHook receivers.
    /// </summary>
    public class WebHookServiceCollectionSetup
    {
        // ??? Does WebHookExceptionFilter need a non-default Order too?
        private static readonly Dictionary<Type, int> SingletonFilters = new Dictionary<Type, int>
        {
                { typeof(WebHookExceptionFilter), 0 },
                { typeof(WebHookGetResponseFilter), WebHookGetResponseFilter.Order },
                { typeof(WebHookPingResponseFilter), WebHookPingResponseFilter.Order },
                { typeof(WebHookVerifyCodeFilter), WebHookSecurityFilter.Order },
                { typeof(WebHookVerifyMethodFilter), WebHookVerifyMethodFilter.Order },
                { typeof(WebHookVerifyRequiredValueFilter), WebHookVerifyRequiredValueFilter.Order },
        };

        /// <summary>
        /// Add services for WebHook receivers.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to update.</param>
        public static void AddWebHookServices(IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, MvcOptionsSetup>());

            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IApplicationModelProvider, WebHookMetadataProvider>());
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IApplicationModelProvider, WebHookModelBindingProvider>());
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IApplicationModelProvider, WebHookRoutingProvider>());

            services.TryAddSingleton<WebHookEventMapperConstraint>();
            services.TryAddSingleton<WebHookReceiverExistsConstraint>();

            services.TryAddSingleton<WebHookReceiverExistsFilter>();

            foreach (var keyValuePair in SingletonFilters)
            {
                services.TryAddSingleton(keyValuePair.Key);
            }
        }

        private class MvcOptionsSetup : IConfigureOptions<MvcOptions>
        {
            /// <inheritdoc />
            public void Configure(MvcOptions options)
            {
                if (options == null)
                {
                    throw new ArgumentNullException(nameof(options));
                }

                foreach (var keyValuePair in SingletonFilters)
                {
                    options.Filters.AddService(keyValuePair.Key, keyValuePair.Value);
                }
            }
        }
    }
}
