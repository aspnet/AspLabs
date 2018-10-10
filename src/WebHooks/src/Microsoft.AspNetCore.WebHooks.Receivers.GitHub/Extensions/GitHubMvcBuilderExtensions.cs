// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up GitHub WebHooks in an <see cref="IMvcBuilder" />.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class GitHubMvcBuilderExtensions
    {
        /// <summary>
        /// <para>
        /// Add GitHub WebHook configuration and services to the specified <paramref name="builder"/>. See
        /// <see href="https://developer.github.com/webhooks/"/> for additional details about GitHub WebHook requests.
        /// </para>
        /// <para>
        /// The '<c>WebHooks:GitHub:SecretKey:default</c>' configuration value contains the secret key for GitHub
        /// WebHook URIs of the form '<c>https://{host}/api/webhooks/incoming/github</c>'.
        /// '<c>WebHooks:GitHub:SecretKey:{id}</c>' configuration values contain secret keys for GitHub WebHook URIs of
        /// the form '<c>https://{host}/api/webhooks/incoming/github/{id}</c>'.
        /// </para>
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder" /> to configure.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static IMvcBuilder AddGitHubWebHooks(this IMvcBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            GitHubServiceCollectionSetup.AddGitHubServices(builder.Services);

            return builder.AddWebHooks();
        }
    }
}
