// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up Bitbucket WebHooks in an <see cref="IMvcBuilder" />.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class BitbucketMvcBuilderExtensions
    {
        /// <summary>
        /// <para>
        /// Add Bitbucket WebHook configuration and services to the specified <paramref name="builder"/>. See
        /// <see href="https://confluence.atlassian.com/bitbucket/manage-webhooks-735643732.html"/> for additional
        /// details about Bitbucket WebHook requests.
        /// </para>
        /// <para>
        /// The '<c>WebHooks:Bitbucket:SecretKey:default</c>' configuration value contains the secret key for Bitbucket
        /// WebHook URIs of the form '<c>https://{host}/api/webhooks/incoming/bitbucket?code={secret key}</c>'.
        /// '<c>WebHooks:Bitbucket:SecretKey:{id}</c>' configuration values contain secret keys for Bitbucket WebHook
        /// URIs of the form '<c>https://{host}/api/webhooks/incoming/bitbucket/{id}?code={secret key}</c>'.
        /// </para>
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder" /> to configure.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static IMvcBuilder AddBitbucketWebHooks(this IMvcBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            BitbucketServiceCollectionSetup.AddBitbucketServices(builder.Services);

            return builder.AddWebHooks();
        }
    }
}
