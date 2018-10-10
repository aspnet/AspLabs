// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.AspNetCore.WebHooks.Routing;

namespace Microsoft.AspNetCore.WebHooks.Filters
{
    /// <summary>
    /// An <see cref="IFilterProvider"/> that adds receiver-specific WebHook filters for non-receiver-specific WebHook
    /// actions i.e. actions with an associated <see cref="GeneralWebHookAttribute"/>.
    /// </summary>
    /// <remarks>
    /// Though this provider does not directly support <see cref="IFilterFactory"/> implementations nor set
    /// <see cref="IFilterContainer.FilterDefinition"/>, it runs before
    /// <see cref="Mvc.Internal.DefaultFilterProvider"/>. Any WebHook <see cref="IFilterMetadata"/> implementation
    /// should be handled as expected.
    /// </remarks>
    public class WebHookFilterProvider : IFilterProvider
    {
        private readonly WebHookMetadataProvider _metadataProvider;

        /// <summary>
        /// Instantiates a new <see cref="WebHookFilterProvider"/> instance.
        /// </summary>
        /// <param name="metadataProvider">The <see cref="WebHookMetadataProvider"/>.</param>
        public WebHookFilterProvider(WebHookMetadataProvider metadataProvider)
        {
            _metadataProvider = metadataProvider;
        }

        /// <inheritdoc />
        /// <value>
        /// Chosen to run this provider early in request processing and before
        /// <see cref="Mvc.Internal.DefaultFilterProvider"/>.
        /// </value>
        public int Order => -1500;

        /// <inheritdoc />
        public void OnProvidersExecuting(FilterProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var actionContext = context.ActionContext;
            if (!actionContext.RouteData.TryGetWebHookReceiverName(out var recieverName))
            {
                // Not a WebHook request.
                return;
            }

            var actionDescriptor = actionContext.ActionDescriptor;
            var bodyTypeMetadataObject = actionDescriptor.Properties[typeof(IWebHookBodyTypeMetadataService)];
            if (bodyTypeMetadataObject is IWebHookBodyTypeMetadataService)
            {
                // Action is receiver-specific. WebHookFilterProvider already added the necessary filters.
                return;
            }

            var filterMetadata = _metadataProvider.GetFilterMetadata(recieverName);
            if (filterMetadata == null)
            {
                // No need for receiver-specific filters.
                return;
            }

            var providerContext = new WebHookFilterMetadataContext(actionDescriptor);
            filterMetadata.AddFilters(providerContext);
            if (providerContext.Results.Count == 0)
            {
                // No receiver-specific filters to add.
                return;
            }

            // Treat all WebHook filters as if they have FilterScope.Action. DefaultFilterProvider will fill in
            // FilterItem.Filter, usually copying FilterDescriptor.Filter.
            var newFilterItems = providerContext.Results
                .Select(filter => new FilterItem(new FilterDescriptor(filter, FilterScope.Action))
                {
                    IsReusable = false,
                });

            // MVC sorts context.Results before calling IFilterProvider implementations. Assume nothing has disturbed
            // that order: Insert results and do not re-sort the whole collection.
            var itemsList = context.Results as List<FilterItem> ?? new List<FilterItem>(context.Results);
            context.Results = itemsList;
            foreach (var newFilterItem in newFilterItems)
            {
                var position = itemsList.BinarySearch(newFilterItem, FilterItemComparer.Instance);
                if (position >= 0)
                {
                    // Insert after the match.
                    itemsList.Insert(position + 1, newFilterItem);
                }
                else
                {
                    itemsList.Insert(~position, newFilterItem);
                }
            }
        }

        /// <inheritdoc />
        public void OnProvidersExecuted(FilterProviderContext context)
        {
            // No-op
        }

        private class FilterItemComparer : IComparer<FilterItem>
        {
            public static FilterItemComparer Instance { get; } = new FilterItemComparer();

            private FilterItemComparer()
            {
            }

            public int Compare(FilterItem x, FilterItem y)
            {
                if (x == null)
                {
                    throw new ArgumentNullException(nameof(x));
                }
                if (y == null)
                {
                    throw new ArgumentNullException(nameof(y));
                }

                // FilterItem.Descriptor cannot be null.
                if (x.Descriptor.Order == y.Descriptor.Order)
                {
                    return x.Descriptor.Scope.CompareTo(y.Descriptor.Scope);
                }

                return x.Descriptor.Order.CompareTo(y.Descriptor.Order);
            }
        }
    }
}
