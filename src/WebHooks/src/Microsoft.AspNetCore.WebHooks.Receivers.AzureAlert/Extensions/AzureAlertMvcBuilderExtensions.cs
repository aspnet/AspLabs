// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up Azure Alert WebHooks in an <see cref="IMvcBuilder" />.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class AzureAlertMvcBuilderExtensions
    {
        /// <summary>
        /// <para>
        /// Add Azure Alert WebHook configuration and services to the specified <paramref name="builder"/>. See
        /// <see href="https://docs.microsoft.com/en-us/azure/monitoring-and-diagnostics/insights-webhooks-alerts"/>
        /// for additional details about Azure Alert WebHook requests.
        /// </para>
        /// <para>
        /// The '<c>WebHooks:AzureAlert:SecretKey:default</c>' configuration value contains the secret key for Azure
        /// Alert WebHook URIs of the form '<c>https://{host}/api/webhooks/incoming/azurealert?code={secret key}</c>'.
        /// '<c>WebHooks:AzureAlert:SecretKey:{id}</c>' configuration values contain secret keys for
        /// Azure Alert WebHook URIs of the form
        /// '<c>https://{host}/api/webhooks/incoming/azurealert/{id}?code={secret key}</c>'.
        /// </para>
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder" /> to configure.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static IMvcBuilder AddAzureAlertWebHooks(this IMvcBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            AzureAlertServiceCollectionSetup.AddAzureAlertServices(builder.Services);

            return builder.AddWebHooks();
        }
    }
}
