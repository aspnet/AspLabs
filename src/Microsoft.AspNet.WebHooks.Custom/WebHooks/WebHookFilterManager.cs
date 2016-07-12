// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides an implementation of <see cref="IWebHookFilterManager"/> which provides the set of 
    /// registered <see cref="WebHookFilter"/> instances.
    /// </summary>
    public class WebHookFilterManager : IWebHookFilterManager
    {
        private readonly IEnumerable<IWebHookFilterProvider> _providers;
        private IDictionary<string, WebHookFilter> _filters;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebHookFilterManager"/> class with the 
        /// given <paramref name="providers"/>.
        /// </summary>
        public WebHookFilterManager(IEnumerable<IWebHookFilterProvider> providers)
        {
            if (providers == null)
            {
                throw new ArgumentNullException(nameof(providers));
            }

            _providers = providers;
        }

        /// <inheritdoc />
        public async Task<IDictionary<string, WebHookFilter>> GetAllWebHookFiltersAsync()
        {
            if (_filters == null)
            {
                IDictionary<string, WebHookFilter> allFilters = new Dictionary<string, WebHookFilter>(StringComparer.OrdinalIgnoreCase);

                // Get all filters from all providers.
                IEnumerable<Task<Collection<WebHookFilter>>> tasks = _providers.Select(p => p.GetFiltersAsync());
                Collection<WebHookFilter>[] providerFilters = await Task.WhenAll(tasks);

                // Flatten filters into one dictionary for lookup.
                foreach (Collection<WebHookFilter> providerFilter in providerFilters)
                {
                    foreach (WebHookFilter filter in providerFilter)
                    {
                        allFilters[filter.Name] = filter;
                    }
                }

                Interlocked.CompareExchange(ref _filters, allFilters, null);
            }

            return _filters;
        }
    }
}
