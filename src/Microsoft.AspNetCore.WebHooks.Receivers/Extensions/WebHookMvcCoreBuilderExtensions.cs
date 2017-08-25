// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebHooks;
using Microsoft.AspNetCore.WebHooks.ApplicationModels;
using Microsoft.AspNetCore.WebHooks.Filters;
using Microsoft.AspNetCore.WebHooks.Routing;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    // TODO: Add IMvcBuilder variant of this class. Do the same for all receiver-specific extension methods too.
    /// <summary>
    /// Extension methods for setting up WebHooks in an <see cref="IMvcCoreBuilder" />.
    /// </summary>
    public static class WebHookMvcCoreBuilderExtensions
    {
        /// <summary>
        /// Add WebHook configuration and services to the specified <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcCoreBuilder" /> to configure.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static IMvcCoreBuilder AddWebHooks(this IMvcCoreBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var services = builder.Services;
            services
                .TryAddEnumerable(ServiceDescriptor.Transient<IApplicationModelProvider, WebHookMetadataProvider>());
            services
                .TryAddEnumerable(ServiceDescriptor.Transient<IApplicationModelProvider, WebHookModelBindingProvider>());
            services
                .TryAddEnumerable(ServiceDescriptor.Transient<IApplicationModelProvider, WebHookRoutingProvider>());

            services.TryAddSingleton<WebHookReceiverExistsConstraint>();
            services.TryAddSingleton<WebHookMultipleEventMapperConstraint>();

            services.TryAddSingleton<IWebHookReceiverConfig, WebHookReceiverConfig>();
            services.TryAddSingleton<WebHookReceiverExistsFilter>();

            // ??? Does WebHookExceptionFilter need a non-default Order too?
            return builder
                .AddSingletonFilter<WebHookExceptionFilter>()
                .AddSingletonFilter<WebHookGetResponseFilter>(WebHookGetResponseFilter.Order)
                .AddSingletonFilter<WebHookPingResponseFilter>(WebHookPingResponseFilter.Order)
                .AddSingletonFilter<WebHookVerifyCodeFilter>(WebHookSecurityFilter.Order)
                .AddSingletonFilter<WebHookVerifyMethodFilter>(WebHookVerifyMethodFilter.Order)
                .AddSingletonFilter<WebHookVerifyRequiredValueFilter>(WebHookVerifyRequiredValueFilter.Order);
        }

        /// <summary>
        /// Add WebHook configuration and services to the specified <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcCoreBuilder" /> to configure.</param>
        /// <param name="setupAction">
        /// An <see cref="Action{WebHookOptions}"/> to configure the provided <see cref="WebHookOptions"/>.
        /// </param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static IMvcCoreBuilder AddWebHooks(this IMvcCoreBuilder builder, Action<WebHookOptions> setupAction)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            builder.AddWebHooks();
            builder.Services.Configure(setupAction);

            return builder;
        }

        /// <summary>
        /// Add <typeparamref name="TFilter"/> as a singleton filter. Register <typeparamref name="TFilter"/> as a
        /// singleton service and add it to <see cref="AspNetCore.Mvc.MvcOptions.Filters"/>.
        /// </summary>
        /// <typeparam name="TFilter">The <see cref="IFilterMetadata"/> type to add.</typeparam>
        /// <param name="builder">The <see cref="IMvcCoreBuilder" /> to configure.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        /// <remarks>This method may be called multiple times for the same <typeparamref name="TFilter"/>.</remarks>
        public static IMvcCoreBuilder AddSingletonFilter<TFilter>(this IMvcCoreBuilder builder)
            where TFilter : class, IFilterMetadata
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.AddSingletonFilter<TFilter>(order: 0);
        }

        /// <summary>
        /// Add <typeparamref name="TFilter"/> as a singleton filter with given <paramref name="order"/>. Register
        /// <typeparamref name="TFilter"/> as a singleton service and add it to
        /// <see cref="AspNetCore.Mvc.MvcOptions.Filters"/>.
        /// </summary>
        /// <typeparam name="TFilter">The <see cref="IFilterMetadata"/> type to add.</typeparam>
        /// <param name="builder">The <see cref="IMvcCoreBuilder" /> to configure.</param>
        /// <param name="order">The <see cref="IOrderedFilter.Order"/> of the new filter.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        /// <remarks>This method may be called multiple times for the same <typeparamref name="TFilter"/>.</remarks>
        public static IMvcCoreBuilder AddSingletonFilter<TFilter>(this IMvcCoreBuilder builder, int order)
            where TFilter : class, IFilterMetadata
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var services = builder.Services;
            services.TryAddSingleton<TFilter>();

            // Ensure the filter is available globally. Filter should no-op for non-WebHook requests.
            builder.AddMvcOptions(options =>
            {
                var filters = options.Filters;

                // Remove existing registration of this type if it has a different Order. IsReusable should always be
                // false (deferring lifetime choices to DI).
                var i = 0;
                var found = false;
                while (i < filters.Count)
                {
                    var filter = filters[i];
                    if (filter is ServiceFilterAttribute serviceFilter)
                    {
                        if (serviceFilter.ServiceType == typeof(TFilter))
                        {
                            if (!serviceFilter.IsReusable && serviceFilter.Order == order)
                            {
                                // Ignore odd cases where collection already contains duplicates.
                                found = true;
                                break;
                            }
                            else
                            {
                                // Replace existing registration with correct Order and IsReusable. Do not increment i.
                                filters.RemoveAt(i);
                                continue;
                            }
                        }
                    }

                    i++;
                }

                if (!found)
                {
                    filters.AddService<TFilter>(order);
                }
            });

            return builder;
        }
    }
}