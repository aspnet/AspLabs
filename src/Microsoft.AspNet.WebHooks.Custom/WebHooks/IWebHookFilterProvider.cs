// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides an abstraction for adding filters that can be used to determine when <see cref="WebHook"/> are triggered.
    /// </summary>
    public interface IWebHookFilterProvider
    {
        /// <summary>
        /// Get the filters for this <see cref="IWebHookFilterProvider"/> implementation so that they be applied to <see cref="WebHook"/>
        /// instances.
        /// </summary>
        /// <returns>A collection of <see cref="WebHookFilter"/> instances.</returns>
        Task<Collection<WebHookFilter>> GetFiltersAsync();
    }
}
