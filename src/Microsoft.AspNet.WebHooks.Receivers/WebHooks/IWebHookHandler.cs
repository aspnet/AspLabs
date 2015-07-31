// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides an abstraction for handling an incoming WebHook request. The <see cref="IWebHookHandler"/>
    /// is independent of which WebHook generator sent the actual request. That is, <see cref="IWebHookHandler"/>
    /// can process WebHook requests originating from any supported <see cref="IWebHookReceiver"/> such as <c>Dropbox</c>
    /// and <c>GitHub</c>, etc.
    /// </summary>
    public interface IWebHookHandler
    {
        /// <summary>
        /// Gets the relative order in which <see cref="IWebHookHandler"/> instances are executed in response to incoming WebHooks. 
        /// The execution order of handler with the same order. By default the handlers are sorted based on their <see cref="Order"/> from lowest
        /// to highest. That is, if there are 3 handlers with <c>Order</c> 50, 10, and 100 then they are executed in the order 10, 50, 100.
        /// </summary>
        int Order { get; }

        /// <summary>
        /// Gets the receiver that this <see cref="IWebHookHandler"/> wants to receive WebHooks from. If <c>null</c> then
        /// it will receive WebHooks from all registered receivers.
        /// </summary>
        string Receiver { get; }

        /// <summary>
        /// Executes the incoming WebHook request.
        /// </summary>
        /// <param name="receiver">The name of the <see cref="IWebHookReceiver"/> which processed the incoming WebHook. The
        /// receiver can for example be <c>dropbox</c> or <c>github</c>.</param>
        /// <param name="context">Provides context for the <see cref="IWebHookHandler"/> for further processing the incoming WebHook.</param>
        Task ExecuteAsync(string receiver, WebHookHandlerContext context);
    }
}
