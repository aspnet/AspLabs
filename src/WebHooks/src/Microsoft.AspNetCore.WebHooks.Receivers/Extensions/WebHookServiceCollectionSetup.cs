// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebHooks;
using Microsoft.AspNetCore.WebHooks.ApplicationModels;
using Microsoft.AspNetCore.WebHooks.Filters;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Methods to add services for WebHook receivers.
    /// </summary>
    internal static class WebHookServiceCollectionSetup
    {
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

            services.TryAddSingleton<WebHookMetadataProvider, DefaultWebHookMetadataProvider>();

            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IApplicationModelProvider, WebHookActionModelFilterProvider>());
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IApplicationModelProvider, WebHookActionModelPropertyProvider>());
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IApplicationModelProvider, WebHookBindingInfoProvider>());
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IApplicationModelProvider, WebHookSelectorModelProvider>());

            services.TryAddSingleton<WebHookReceiverExistsFilter>();
            services.TryAddSingleton<WebHookVerifyMethodFilter>();
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IFilterProvider, WebHookFilterProvider>());

            services.TryAddSingleton<IWebHookRequestReader, WebHookRequestReader>();
        }
    }
}
