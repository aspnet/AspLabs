// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Various extension methods for the ASP.NET Web API <see cref="IWebHookManager"/> interface.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class WebHookManagerExtensions
    {
        /// <summary>
        /// Submits a notification to all matching registered WebHooks across all users. To match, the <see cref="WebHook"/> must 
        /// have a filter that matches one or more of the actions provided for the notification.
        /// </summary>
        /// <param name="manager">The <see cref="IWebHookManager"/> instance.</param>
        /// <param name="action">The action describing the notification.</param>
        /// <param name="data">Optional additional data to include in the WebHook request.</param>
        /// <returns>The number of <see cref="WebHook"/> instances that were selected and subsequently notified about the actions.</returns>
        public static Task<int> NotifyAllAsync(this IWebHookManager manager, string action, object data)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            var notifications = new NotificationDictionary[] { new NotificationDictionary(action, data) };
            return manager.NotifyAllAsync(notifications, predicate: null);
        }

        /// <summary>
        /// Submits a notification to all matching registered WebHooks across all users. To match, the <see cref="WebHook"/> must 
        /// have a filter that matches one or more of the actions provided for the notification.
        /// </summary>
        /// <param name="manager">The <see cref="IWebHookManager"/> instance.</param>
        /// <param name="action">The action describing the notification.</param>
        /// <param name="data">Optional additional data to include in the WebHook request.</param>
        /// <param name="predicate">A function to test each <see cref="WebHook"/> to see whether it fulfills the condition. The
        /// predicate is passed the <see cref="WebHook"/> and the user who registered it. If the predicate returns <c>true</c> then
        /// the <see cref="WebHook"/> is included; otherwise it is not.</param>
        /// <returns>The number of <see cref="WebHook"/> instances that were selected and subsequently notified about the actions.</returns>
        public static Task<int> NotifyAllAsync(this IWebHookManager manager, string action, object data, Func<WebHook, string, bool> predicate)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            var notifications = new NotificationDictionary[] { new NotificationDictionary(action, data) };
            return manager.NotifyAllAsync(notifications, predicate);
        }

        /// <summary>
        /// Submits a notification to all matching registered WebHooks across all users. To match, the <see cref="WebHook"/> must 
        /// have a filter that matches one or more of the actions provided for the notification.
        /// </summary>
        /// <param name="manager">The <see cref="IWebHookManager"/> instance.</param>
        /// <param name="notifications">The set of notifications to include in the WebHook.</param>
        /// <returns>The number of <see cref="WebHook"/> instances that were selected and subsequently notified about the actions.</returns>
        public static Task<int> NotifyAllAsync(this IWebHookManager manager, params NotificationDictionary[] notifications)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            return manager.NotifyAllAsync(notifications, predicate: null);
        }
    }
}
