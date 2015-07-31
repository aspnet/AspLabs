// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides an abstraction for managing <see cref="IWebHookReceiver"/> instances which process incoming WebHook requests.
    /// </summary>
    public interface IWebHookReceiverManager
    {
        /// <summary>
        /// Gets the <see cref="IWebHookReceiver"/> matching the given <paramref name="receiverName"/>.
        /// </summary>
        /// <param name="receiverName">Case-insensitive name of storage provider, e.g. <c>Dropbox</c>.</param>
        /// <returns>A <see cref="IWebHookReceiver"/> representing the storage or <c>null</c> if no match is found.</returns>
        IWebHookReceiver GetReceiver(string receiverName);
    }
}
