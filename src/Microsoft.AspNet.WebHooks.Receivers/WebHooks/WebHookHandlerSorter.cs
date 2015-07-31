// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides an implementation of <see cref="IWebHookHandlerSorter"/> which sorts registered
    /// <see cref="IWebHookHandler"/> instances based on their designated <see cref="IWebHookHandler.Order"/>.
    /// </summary>
    public class WebHookHandlerSorter : IWebHookHandlerSorter
    {
        /// <inheritdoc />
        public IEnumerable<IWebHookHandler> SortHandlers(IEnumerable<IWebHookHandler> handlers)
        {
            if (handlers == null)
            {
                throw new ArgumentNullException("handlers");
            }

            return handlers.OrderBy(h => h.Order).ToArray();
        }
    }
}
