// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using Microsoft.AspNetCore.WebHooks.Internal;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up Trello WebHooks in an <see cref="IMvcCoreBuilder" />.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class TrelloMvcCoreBuilderExtensions
    {
        /// <summary>
        /// <para>
        /// Add Trello WebHook configuration and services to the specified <paramref name="builder"/>. See
        /// <see href="https://developers.trello.com/page/webhooks"/> for additional details about Trello WebHook
        /// requests.
        /// </para>
        /// <para>
        /// The '<c>WebHooks:Trello:SecretKey:default</c>' configuration value contains the secret key for Trello
        /// WebHook URIs of the form '<c>https://{host}/api/webhooks/incoming/trello</c>'.
        /// '<c>WebHooks:Trello:SecretKey:{id}</c>' configuration values contain secret keys for Trello WebHook URIs of
        /// the form '<c>https://{host}/api/webhooks/incoming/trello/{id}</c>'. Find your secret key at
        /// <see href="https://trello.com/app-key"/> under <c>OAuth</c>.
        /// </para>
        /// </summary>
        /// <param name="builder">The <see cref="IMvcCoreBuilder" /> to configure.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static IMvcCoreBuilder AddTrelloWebHooks(this IMvcCoreBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            TrelloServiceCollectionSetup.AddTrelloServices(builder.Services);

            return builder
                .AddJsonFormatters()
                .AddWebHooks();
        }
    }
}
