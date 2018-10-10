// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.WebHooks;
using Microsoft.AspNetCore.WebHooks.Filters;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.AspNetCore.WebHooks.ModelBinding;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Methods to add services for the Salesforce receiver.
    /// </summary>
    internal static class SalesforceServiceCollectionSetup
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

            services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, MvcOptionsSetup>());
            WebHookMetadata.Register<SalesforceMetadata>(services);
            services.TryAddSingleton<ISalesforceResultCreator, SalesforceResultCreator>();
            services.TryAddSingleton<SalesforceVerifyOrganizationIdFilter>();
        }

        private class MvcOptionsSetup : IConfigureOptions<MvcOptions>
        {
            private readonly ILoggerFactory _loggerFactory;
            private readonly IHttpRequestStreamReaderFactory _readerFactory;

            public MvcOptionsSetup(ILoggerFactory loggerFactory, IHttpRequestStreamReaderFactory readerFactory)
            {
                _loggerFactory = loggerFactory;
                _readerFactory = readerFactory;
            }

            public void Configure(MvcOptions options)
            {
                if (options == null)
                {
                    throw new ArgumentNullException(nameof(options));
                }

                // Ensure this binder is placed before the BodyModelBinderProvider, the most likely provider to match
                // the XElement type.
                var provider = new SalesforceNotificationsModelBinderProvider(_loggerFactory, options, _readerFactory);
                options.ModelBinderProviders.Insert(0, provider);
            }
        }
    }
}
