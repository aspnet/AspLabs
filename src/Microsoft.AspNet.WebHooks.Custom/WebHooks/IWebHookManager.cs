// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides an abstraction for launching WebHooks based on events happening in the system. When 
    /// the <see cref="NotifyAsync(string, IEnumerable{NotificationDictionary}, Func{WebHook, string, bool})"/> method is called, 
    /// all registered WebHooks with matching filters will launch indicating to the recipient of the WebHook that an event happened.
    /// </summary>
    public interface IWebHookManager
    {
        /// <summary>
        /// Verifies that the URI of the given <paramref name="webHook"/> is reachable and responds with the expected
        /// data in response to an echo request. If a correct response can not be obtained then an <see cref="System.Exception"/>
        /// is thrown with a detailed description of the problem.
        /// </summary>
        /// <param name="webHook">The <see cref="WebHook"/> to verify</param>
        Task VerifyWebHookAsync(WebHook webHook);

        /// <summary>
        /// Submits a notification to all registered WebHooks for a given <paramref name="user"/> with one or 
        /// more matching filters. For active WebHooks with matching filters, an HTTP request will be sent to the 
        /// designated WebHook URI with information about the action.
        /// </summary>
        /// <param name="user">The user for which to lookup and dispatch matching WebHooks.</param>
        /// <param name="notifications">The set of notifications to include in the WebHook request.</param>
        /// <param name="predicate">An optional function to test each <see cref="WebHook"/> to see whether it fulfills the condition. The
        /// predicate is passed the <see cref="WebHook"/> and the user who registered it. If the predicate returns <c>true</c> then
        /// the <see cref="WebHook"/> is included; otherwise it is not.</param>
        /// <returns>The number of <see cref="WebHook"/> instances that were selected and subsequently notified about the actions.</returns>
        Task<int> NotifyAsync(string user, IEnumerable<NotificationDictionary> notifications, Func<WebHook, string, bool> predicate);

        /// <summary>
        /// Submits a notification to all users with subscriptions that match the given <paramref name="predicate"/>. For active 
        /// WebHooks with matching filters, regardless of user, an HTTP request will be sent to the designated WebHook URI with 
        /// information about the action.
        /// </summary>
        /// <param name="notifications">The set of notifications to include in the WebHook request.</param>
        /// <param name="predicate">An optional function to test each <see cref="WebHook"/> to see whether it fulfills the condition. The
        /// predicate is passed the <see cref="WebHook"/> and the user who registered it. If the predicate returns <c>true</c> then
        /// the <see cref="WebHook"/> is included; otherwise it is not.</param>
        /// <returns>The number of <see cref="WebHook"/> instances that were selected and subsequently notified about the actions.</returns>
        Task<int> NotifyAllAsync(IEnumerable<NotificationDictionary> notifications, Func<WebHook, string, bool> predicate);
    }
}
