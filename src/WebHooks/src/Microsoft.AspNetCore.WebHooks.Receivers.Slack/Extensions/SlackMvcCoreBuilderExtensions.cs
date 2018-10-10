// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up Slack WebHooks in an <see cref="IMvcCoreBuilder" />.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class SlackMvcCoreBuilderExtensions
    {
        /// <summary>
        /// <para>
        /// Add Slack WebHook configuration and services to the specified <paramref name="builder"/>. See
        /// <see href="https://api.slack.com/custom-integrations/outgoing-webhooks"/> for additional details about
        /// Slack WebHook requests.
        /// </para>
        /// <para>
        /// The '<c>WebHooks:Slack:SecretKey:default</c>' configuration value contains the secret key for Slack WebHook
        /// URIs of the form '<c>https://{host}/api/webhooks/incoming/slack</c>'.
        /// '<c>WebHooks:Slack:SecretKey:{id}</c>' configuration values contain secret keys for Slack WebHook URIs of
        /// the form '<c>https://{host}/api/webhooks/incoming/slack/{id}</c>'.
        /// </para>
        /// </summary>
        /// <param name="builder">The <see cref="IMvcCoreBuilder" /> to configure.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static IMvcCoreBuilder AddSlackWebHooks(this IMvcCoreBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            SlackServiceCollectionSetup.AddSlackServices(builder.Services);

            // While requests contain HTML form URL-encoded data, responses are JSON.
            return builder
                .AddJsonFormatters()
                .AddWebHooks();
        }
    }
}
