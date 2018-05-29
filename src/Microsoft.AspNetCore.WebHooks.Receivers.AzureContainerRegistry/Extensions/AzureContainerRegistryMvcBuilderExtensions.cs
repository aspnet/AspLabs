// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up AzureContainerRegistry WebHooks in an <see cref="IMvcBuilder" />.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class AzureContainerRegistryMvcBuilderExtensions
    {
        /// <summary>
        /// <para>
        /// Add AzureContainerRegistry WebHook configuration and services to the specified <paramref name="builder"/>. See
        /// <see href="https://docs.microsoft.com/en-us/azure/container-registry/container-registry-webhook-reference"/> for additional details about AzureContainerRegistry WebHook requests.
        /// </para>
        /// <para>
        /// The '<c>WebHooks:AzureContainerRegistry:SecretKey:default</c>' configuration value contains the secret key for AzureContainerRegistry
        /// WebHook URIs of the form '<c>https://{host}/api/webhooks/incoming/azurecontainerregistry</c>'.
        /// '<c>WebHooks:AzureContainerRegistry:SecretKey:{id}</c>' configuration values contain secret keys for AzureContainerRegistry WebHook URIs of
        /// the form '<c>https://{host}/api/webhooks/incoming/azurecontainerregistry/{id}</c>'.
        /// </para>
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder" /> to configure.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static IMvcBuilder AddAzureContainerRegistryWebHooks(this IMvcBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            AzureContainerRegistryServiceCollectionSetup.AddAzureContainerRegistryServices(builder.Services);

            return builder.AddWebHooks();
        }
    }
}
