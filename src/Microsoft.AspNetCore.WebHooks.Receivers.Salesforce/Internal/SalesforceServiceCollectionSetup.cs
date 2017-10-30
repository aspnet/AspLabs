// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.AspNetCore.WebHooks.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.WebHooks.Internal
{
    /// <summary>
    /// Methods to add services for the Salesforce receiver.
    /// </summary>
    public static class SalesforceServiceCollectionSetup
    {
        /// <summary>
        /// Add services for the Salesforce receiver.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to update.</param>
        public static void AddSalesforceServices(IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, MvcOptionsSetup>());
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IWebHookMetadata, SalesforceMetadata>());
            services.TryAddSingleton<ISalesforceResultCreator, SalesforceResultCreator>();
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

                // Ensure this binder is placed before the BodyModelBinderProvider, the most likely provider to match
                // the XElement type.
                options.ModelBinderProviders.Insert(0, new SalesforceNotificationsModelBinderProvider());
            }
        }
    }
}
