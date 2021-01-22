// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up Azure DevOps WebHooks in an <see cref="IMvcBuilder" />.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class AzureDevOpsMvcBuilderExtensions
    {
        /// <summary>
        /// <para>
        /// Add Azure DevOps WebHook configuration and services to the specified <paramref name="builder"/>. See
        /// <see href="https://docs.microsoft.com/en-us/azure/devops/service-hooks/services/webhooks?view=azure-devops"/>
        /// for additional details about Azure DevOps WebHook requests.
        /// </para>
        /// <para>
        /// The '<c>WebHooks:AzureDevOps:SecretKey:default</c>' configuration value contains the secret key for Azure
        /// DevOps WebHook URIs of the form '<c>https://{host}/api/webhooks/incoming/azuredevops?code={secret key}</c>'.
        /// '<c>WebHooks:AzureDevOps:SecretKey:{id}</c>' configuration values contain secret keys for
        /// Azure DevOps WebHook URIs of the form
        /// '<c>https://{host}/api/webhooks/incoming/azuredevops/{id}?code={secret key}</c>'.
        /// </para>
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder" /> to configure.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static IMvcBuilder AddAzureDevOpsWebHooks(this IMvcBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            AzureDevOpsServiceCollectionSetup.AddAzureDevOpsServices(builder.Services);

            return builder.AddWebHooks();
        }
    }
}
