// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up WordPress WebHooks in an <see cref="IMvcBuilder" />.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class WordPressMvcBuilderExtensions
    {
        /// <summary>
        /// <para>
        /// Add WordPress WebHook configuration and services to the specified <paramref name="builder"/>. See
        /// <see href="https://en.support.wordpress.com/webhooks/"/> for additional details about WordPress WebHook
        /// requests.
        /// </para>
        /// <para>
        /// The '<c>WebHooks:WordPress:SecretKey:default</c>' configuration value contains the secret key for WordPress
        /// WebHook URIs of the form '<c>https://{host}/api/webhooks/incoming/wordpress?code={secret key}</c>'.
        /// '<c>WebHooks:WordPress:SecretKey:{id}</c>' configuration values contain secret keys for
        /// WordPress WebHook URIs of the form
        /// '<c>https://{host}/api/webhooks/incoming/wordpress/{id}?code={secret key}</c>'.
        /// </para>
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder" /> to configure.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static IMvcBuilder AddWordPressWebHooks(this IMvcBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            WordPressServiceCollectionSetup.AddWordPressServices(builder.Services);

            return builder.AddWebHooks();
        }
    }
}
