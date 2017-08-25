// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using Microsoft.AspNetCore.WebHooks;
using Microsoft.AspNetCore.WebHooks.Filters;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.AspNetCore.WebHooks.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up Salesforce WebHooks in an <see cref="IMvcCoreBuilder" />.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class SalesforceMvcCoreBuilderExtensions
    {
        private static readonly Action<WebHookOptions> OptionsSetupAction = SetupWebHookOptions;

        /// <summary>
        /// Add Salesforce WebHook configuration and services to the specified <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcCoreBuilder" /> to configure.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static IMvcCoreBuilder AddSalesforceWebHooks(this IMvcCoreBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var services = builder.Services;
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IWebHookMetadata, SalesforceMetadata>());
            services.TryAddSingleton<ISalesforceResultCreator, SalesforceResultCreator>();

            // ??? Are the [DataContract] formatters also needed? XmlSerializer is enough for at least XElement.
            // ??? Does SalesforceAcknowledgmentFilter need a non-default Order too?
            return builder
                .AddXmlSerializerFormatters()
                .AddWebHooks(OptionsSetupAction)
                .AddSingletonFilter<SalesforceAcknowledgmentFilter>()
                .AddSingletonFilter<SalesforceVerifyOrganizationIdFilter>(WebHookSecurityFilter.Order);
        }

        /// <summary>
        /// Add Salesforce WebHook configuration and services to the specified <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcCoreBuilder" /> to configure.</param>
        /// <param name="setupAction">
        /// An <see cref="Action{WebHookOptions}"/> to configure the provided <see cref="WebHookOptions"/>.
        /// </param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static IMvcCoreBuilder AddSalesforceWebHooks(
            this IMvcCoreBuilder builder,
            Action<WebHookOptions> setupAction)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            builder.AddSalesforceWebHooks();
            builder.Services.Configure(setupAction);

            return builder;
        }

        private static void SetupWebHookOptions(WebHookOptions options)
        {
            if (!options.HttpContextItemsTypes.Contains(typeof(SalesforceNotifications)))
            {
                options.HttpContextItemsTypes.Add(typeof(SalesforceNotifications));
            }
        }
    }
}
