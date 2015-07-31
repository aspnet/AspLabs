// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides an abstraction for sorting registered <see cref="IWebHookHandler"/> instances based on their relative ordering.
    /// </summary>
    public interface IWebHookHandlerSorter
    {
        /// <summary>
        /// Sorts the provided <paramref name="handlers"/> hence controlling the order in which they will get executed upon
        /// incoming WebHook requests. The sorter can use the Order property as input for the relative ordering.
        /// </summary>
        /// <param name="handlers">The set of <see cref="IWebHookHandler"/> to sort.</param>
        /// <returns>The sorted list of <see cref="IWebHookHandler"/> instances.</returns>
        IEnumerable<IWebHookHandler> SortHandlers(IEnumerable<IWebHookHandler> handlers);
    }
}
