// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using Microsoft.AspNetCore.WebHooks.Internal;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up Dynamics CRM WebHooks in an <see cref="IMvcCoreBuilder" />.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class DynamicsCRMMvcCoreBuilderExtensions
    {
        /// <summary>
        /// <para>
        /// Add Dynamics CRM WebHook configuration and services to the specified <paramref name="builder"/>. See
        /// <see href="https://go.microsoft.com/fwlink/?LinkId=722218"/> for additional details about Dynamics CRM
        /// WebHook requests.
        /// </para>
        /// <para>
        /// The '<c>WebHooks:DynamicsCrm:SecretKey:default</c>' configuration value contains the secret key for
        /// Dynamics CRM WebHook URIs of the form
        /// '<c>https://{host}/api/webhooks/incoming/dynamicscrm?code={secret key}</c>'.
        /// '<c>WebHooks:DynamicsCrm:SecretKey:{id}</c>' configuration values contain secret keys for Dynamics CRM
        /// WebHook URIs of the form '<c>https://{host}/api/webhooks/incoming/dynamicscrm/{id}?code={secret key}</c>'.
        /// </para>
        /// </summary>
        /// <param name="builder">The <see cref="IMvcCoreBuilder" /> to configure.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static IMvcCoreBuilder AddDynamicsCRMWebHooks(this IMvcCoreBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            DynamicsCRMServiceCollectionSetup.AddDynamicsCRMServices(builder.Services);

            return builder
                .AddJsonFormatters()
                .AddWebHooks();
        }
    }
}
