// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using Microsoft.AspNetCore.WebHooks.Internal;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up Stripe WebHooks in an <see cref="IMvcCoreBuilder" />.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class StripeMvcCoreBuilderExtensions
    {
        /// <summary>
        /// <para>
        /// Add Stripe WebHook configuration and services to the specified <paramref name="builder"/>. See
        /// <see href="https://stripe.com/docs/webhooks"/> for additional details about Stripe WebHook requests. See
        /// <see href="https://stripe.com/docs/connect/webhooks"/> for additional details about Stripe Connect WebHook
        /// requests. And, see <see href="https://stripe.com/docs/api/dotnet#events"/> for additional details about
        /// Stripe WebHook request payloads.
        /// </para>
        /// <para>
        /// The '<c>WebHooks:Stripe:SecretKey:default</c>' configuration value contains the signing secret for Stripe
        /// WebHook URIs of the form '<c>https://{host}/api/webhooks/incoming/stripe</c>'.
        /// '<c>WebHooks:Stripe:SecretKey:{id}</c>' configuration values contain signing secret for Stripe WebHook URIs
        /// of the form '<c>https://{host}/api/webhooks/incoming/stripe/{id}</c>'. For details about Stripe signing
        /// secrets, see <see href="https://stripe.com/docs/webhooks#signatures"/>.
        /// </para>
        /// </summary>
        /// <param name="builder">The <see cref="IMvcCoreBuilder" /> to configure.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static IMvcCoreBuilder AddStripeWebHooks(this IMvcCoreBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            StripeServiceCollectionSetup.AddStripeServices(builder.Services);

            return builder
                .AddJsonFormatters()
                .AddWebHooks();
        }
    }
}
