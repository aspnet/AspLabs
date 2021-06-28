// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up Intercom WebHooks in an <see cref="IMvcCoreBuilder" />.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class IntercomMvcCoreBuilderExtensions
    {
        /// <summary>
        /// <para>
        /// Add Intercom WebHook configuration and services to the specified <paramref name="builder"/>. See
        /// <see href="https://developers.intercom.com/building-apps/docs/webhooks"/> for additional details about Intercom WebHook requests.
        /// </para>
        /// <para>
        /// The '<c>WebHooks:Intercom:SecretKey:default</c>' configuration value contains the secret key for Intercom
        /// WebHook URIs of the form '<c>https://{host}/api/webhooks/incoming/intercom</c>'.
        /// '<c>WebHooks:Intercom:SecretKey:{id}</c>' configuration values contain secret keys for Intercom WebHook URIs of
        /// the form '<c>https://{host}/api/webhooks/incoming/intercom/{id}</c>'.
        /// </para>
        /// </summary>
        /// <param name="builder">The <see cref="IMvcCoreBuilder" /> to configure.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static IMvcCoreBuilder AddIntercomWebHooks(this IMvcCoreBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            IntercomServiceCollectionSetup.AddIntercomServices(builder.Services);

            return builder
                .AddJsonFormatters()
                .AddWebHooks();
        }
    }
}
