// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using Microsoft.AspNetCore.WebHooks.Internal;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up Kudu WebHooks in an <see cref="IMvcCoreBuilder" />.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class KuduMvcCoreBuilderExtensions
    {
        /// <summary>
        /// <para>
        /// Add Kudu WebHook configuration and services to the specified <paramref name="builder"/>. See
        /// <see href="https://github.com/projectkudu/kudu/wiki/Web-hooks"/> for additional details about Kudu WebHook
        /// requests.
        /// </para>
        /// <para>
        /// The '<c>WebHooks:Kudu:SecretKey:default</c>' configuration value contains the secret key for Kudu WebHook
        /// URIs of the form '<c>https://{host}/api/webhooks/incoming/kudu?code={secret key}</c>'.
        /// '<c>WebHooks:Kudu:SecretKey:{id}</c>' configuration values contain secret keys for Kudu WebHook URIs of the
        /// form '<c>https://{host}/api/webhooks/incoming/kudu/{id}?code={secret key}</c>'.
        /// </para>
        /// </summary>
        /// <param name="builder">The <see cref="IMvcCoreBuilder" /> to configure.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static IMvcCoreBuilder AddKuduWebHooks(this IMvcCoreBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            KuduServiceCollectionSetup.AddKuduServices(builder.Services);

            return builder
                .AddJsonFormatters()
                .AddWebHooks();
        }
    }
}
