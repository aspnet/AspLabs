// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebHooks.Filters;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.WebHooks.Internal
{
    /// <summary>
    /// Methods to add services for the GitHub receiver.
    /// </summary>
    public static class GitHubServiceCollectionSetup
    {
        /// <summary>
        /// Add services for the GitHub receiver.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to update.</param>
        public static void AddGitHubServices(IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, MvcOptionsSetup>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IWebHookEventMetadata, GitHubMetadata>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IWebHookPingRequestMetadata, GitHubMetadata>());

            services.TryAddSingleton<GitHubVerifySignatureFilter>();
        }

        private class MvcOptionsSetup : IConfigureOptions<MvcOptions>
        {
            /// <inheritdoc />
            public void Configure(MvcOptions options)
            {
                if (options == null)
                {
                    throw new ArgumentNullException(nameof(options));
                }

                options.Filters.AddService<GitHubVerifySignatureFilter>(WebHookSecurityFilter.Order);
            }
        }
    }
}
