// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up MailChimp WebHooks in an <see cref="IMvcCoreBuilder" />.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class MailChimpMvcCoreBuilderExtensions
    {
        /// <summary>
        /// <para>
        /// Add MailChimp WebHook configuration and services to the specified <paramref name="builder"/>. See
        /// <see href="https://developer.mailchimp.com/documentation/mailchimp/guides/about-webhooks/"/> for additional
        /// details about MailChimp WebHook requests.
        /// </para>
        /// <para>
        /// The '<c>WebHooks:MailChimp:SecretKey:default</c>' configuration value contains the secret key for MailChimp
        /// WebHook URIs of the form '<c>https://{host}/api/webhooks/incoming/mailchimp</c>'.
        /// '<c>WebHooks:MailChimp:SecretKey:{id}</c>' configuration values contain secret keys for MailChimp WebHook
        /// URIs of the form '<c>https://{host}/api/webhooks/incoming/mailchimp/{id}</c>'.
        /// </para>
        /// </summary>
        /// <param name="builder">The <see cref="IMvcCoreBuilder" /> to configure.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static IMvcCoreBuilder AddMailChimpWebHooks(this IMvcCoreBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            MailChimpServiceCollectionSetup.AddMailChimpServices(builder.Services);

            return builder.AddWebHooks();
        }
    }
}
