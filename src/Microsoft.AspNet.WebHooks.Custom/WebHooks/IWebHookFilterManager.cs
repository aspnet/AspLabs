// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides an abstraction for managing all registered <see cref="IWebHookFilterProvider"/> instances.
    /// </summary>
    public interface IWebHookFilterManager
    {
        /// <summary>
        /// Gets a <see cref="Dictionary{TKey,TValue}"/> of all registered <see cref="WebHookFilter"/> instances 
        /// provided by registered <see cref="IWebHookFilterProvider"/> instances.
        /// </summary>
        /// <returns>An <see cref="IDictionary{TKey, TValue}"/> of <see cref="WebHookFilter"/> instances keyed by name.</returns>
        Task<IDictionary<string, WebHookFilter>> GetAllWebHookFiltersAsync();
    }
}
