// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up Dropbox WebHooks in an <see cref="IMvcCoreBuilder" />.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class DropboxMvcCoreBuilderExtensions
    {
        /// <summary>
        /// <para>
        /// Add Dropbox WebHook configuration and services to the specified <paramref name="builder"/>. See
        /// <see href="https://www.dropbox.com/developers/webhooks/docs"/> for additional details about Dropbox WebHook
        /// requests.
        /// </para>
        /// <para>
        /// The '<c>WebHooks:Dropbox:SecretKey:default</c>' configuration value contains the secret key for Dropbox
        /// WebHook URIs of the form '<c>https://{host}/api/webhooks/incoming/dropbox</c>'.
        /// '<c>WebHooks:Dropbox:SecretKey:{id}</c>' configuration values contain secret keys for Dropbox WebHook URIs
        /// of the form '<c>https://{host}/api/webhooks/incoming/dropbox/{id}</c>'.
        /// </para>
        /// </summary>
        /// <param name="builder">The <see cref="IMvcCoreBuilder" /> to configure.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static IMvcCoreBuilder AddDropboxWebHooks(this IMvcCoreBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            DropboxServiceCollectionSetup.AddDropboxServices(builder.Services);

            return builder
                .AddJsonFormatters()
                .AddWebHooks();
        }
    }
}
