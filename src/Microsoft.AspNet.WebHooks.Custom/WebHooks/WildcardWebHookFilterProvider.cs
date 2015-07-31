// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks.Properties;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Defines a default wildcard <see cref="WebHookFilter"/> which matches all filters.
    /// </summary>
    public class WildcardWebHookFilterProvider : IWebHookFilterProvider
    {
        private const string WildcardName = "*";

        private static readonly Collection<WebHookFilter> Filters = new Collection<WebHookFilter>
        {
            new WebHookFilter { Name = WildcardName, Description = CustomResources.Filter_WildcardDescription },
        };

        /// <summary>
        /// Gets the name of the <see cref="WebHookFilter"/> registered by this <see cref="IWebHookFilterProvider"/>.
        /// </summary>
        public static string Name
        {
            get { return WildcardName; }
        }

        /// <inheritdoc />
        public Task<Collection<WebHookFilter>> GetFiltersAsync()
        {
            return Task.FromResult(Filters);
        }
    }
}
