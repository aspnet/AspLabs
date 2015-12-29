// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides an abstraction for sending out WebHooks as provided by <see cref="IWebHookManager"/>. Implementation
    /// can control the format of the WebHooks as well as how they are sent including retry policies and error handling.
    /// </summary>
    public interface IWebHookSender
    {
        /// <summary>
        /// Sends out the given collection of <paramref name="workItems"/> using whatever mechanism defined by the
        /// <see cref="IWebHookSender"/> implementation.
        /// </summary>
        /// <param name="workItems">The collection of <see cref="WebHookWorkItem"/> instances to process.</param>
        Task SendWebHookWorkItemsAsync(IEnumerable<WebHookWorkItem> workItems);
    }
}
