// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.WebHooks.Metadata;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Methods to add services for the AzureContainerRegistry receiver.
    /// </summary>
    internal static class AzureContainerRegistryServiceCollectionSetup
    {
        /// <summary>
        /// Add services for the AzureContainerRegistry receiver.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to update.</param>
        public static void AddAzureContainerRegistryServices(IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            WebHookMetadata.Register<AzureContainerRegistryMetadata>(services);
        }
    }
}
