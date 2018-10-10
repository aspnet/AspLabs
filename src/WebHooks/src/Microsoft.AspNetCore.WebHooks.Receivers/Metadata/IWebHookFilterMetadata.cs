// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebHooks.ApplicationModels;

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// <para>
    /// Metadata providing additional <see cref="IFilterMetadata"/> instances for a WebHook action. Implemented in a
    /// <see cref="IWebHookMetadata"/> service for receivers that require filters beyond the default filters added
    /// based on other <see cref="IWebHookMetadata"/>.
    /// </para>
    /// <para>
    /// <see cref="WebHookActionModelFilterProvider"/> invokes <see cref="IWebHookFilterMetadata"/> services to add
    /// filters to the <see cref="ActionModel"/> of receiver-specific WebHook actions.
    /// </para>
    /// </summary>
    public interface IWebHookFilterMetadata : IWebHookMetadata, IWebHookReceiver
    {
        /// <summary>
        /// Add <see cref="IFilterMetadata"/> instances to <see cref="WebHookFilterMetadataContext.Results"/> of
        /// <paramref name="context"/>.
        /// </summary>
        /// <param name="context">The <see cref="WebHookFilterMetadataContext"/> for the action.</param>
        /// <remarks>
        /// Added filters should not check applicability before executing e.g. no need to get the receiver name from
        /// <see cref="RouteData"/> or to call the filter's own <see cref="IWebHookReceiver.IsApplicable"/> method
        /// within <see cref="IResourceFilter.OnResourceExecuting"/>.
        /// </remarks>
        void AddFilters(WebHookFilterMetadataContext context);
    }
}
