// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks;

namespace System.Web.Mvc
{
    /// <summary>
    /// Various extension methods for the ASP.NET MVC <see cref="Controller"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ControllerExtensions
    {
        /// <summary>
        /// Submits a notification to all matching registered WebHooks. To match, the <see cref="WebHook"/> must be registered by the
        /// current <see cref="Controller.User"/> and have a filter that matches one or more of the actions provided for the notification.
        /// </summary>
        /// <param name="controller">The MVC <see cref="Controller"/> instance.</param>
        /// <param name="action">The action describing the notification.</param>
        /// <param name="data">Optional additional data to include in the WebHook request.</param>
        /// <returns>The number of <see cref="WebHook"/> instances that were selected and subsequently notified about the actions.</returns>
        public static Task<int> NotifyAsync(this Controller controller, string action, object data)
        {
            var notifications = new NotificationDictionary[] { new NotificationDictionary(action, data) };
            return NotifyAsync(controller, notifications, predicate: null);
        }

        /// <summary>
        /// Submits a notification to all matching registered WebHooks. To match, the <see cref="WebHook"/> must be registered by the
        /// current <see cref="Controller.User"/> and have a filter that matches one or more of the actions provided for the notification.
        /// </summary>
        /// <param name="controller">The MVC <see cref="Controller"/> instance.</param>
        /// <param name="action">The action describing the notification.</param>
        /// <param name="data">Optional additional data to include in the WebHook request.</param>
        /// <param name="predicate">A function to test each <see cref="WebHook"/> to see whether it fulfills the condition. The
        /// predicate is passed the <see cref="WebHook"/> and the user who registered it. If the predicate returns <c>true</c> then
        /// the <see cref="WebHook"/> is included; otherwise it is not.</param>
        /// <returns>The number of <see cref="WebHook"/> instances that were selected and subsequently notified about the actions.</returns>
        public static Task<int> NotifyAsync(this Controller controller, string action, object data, Func<WebHook, string, bool> predicate)
        {
            var notifications = new NotificationDictionary[] { new NotificationDictionary(action, data) };
            return NotifyAsync(controller, notifications, predicate);
        }

        /// <summary>
        /// Submits a notification to all matching registered WebHooks. To match, the <see cref="WebHook"/> must be registered by the
        /// current <see cref="Controller.User"/> and have a filter that matches one or more of the actions provided for the notification.
        /// </summary>
        /// <param name="controller">The MVC <see cref="Controller"/> instance.</param>
        /// <param name="notifications">The set of notifications to include in the WebHook.</param>
        /// <returns>The number of <see cref="WebHook"/> instances that were selected and subsequently notified about the actions.</returns>
        public static Task<int> NotifyAsync(this Controller controller, params NotificationDictionary[] notifications)
        {
            return NotifyAsync(controller, notifications, predicate: null);
        }

        /// <summary>
        /// Submits a notification to all matching registered WebHooks. To match, the <see cref="WebHook"/> must be registered by the
        /// current <see cref="Controller.User"/> and have a filter that matches one or more of the actions provided for the notification.
        /// </summary>
        /// <param name="controller">The MVC <see cref="Controller"/> instance.</param>
        /// <param name="notifications">The set of notifications to include in the WebHook.</param>
        /// <param name="predicate">A function to test each <see cref="WebHook"/> to see whether it fulfills the condition. The
        /// predicate is passed the <see cref="WebHook"/> and the user who registered it. If the predicate returns <c>true</c> then
        /// the <see cref="WebHook"/> is included; otherwise it is not.</param>
        /// <returns>The number of <see cref="WebHook"/> instances that were selected and subsequently notified about the actions.</returns>
        public static async Task<int> NotifyAsync(this Controller controller, IEnumerable<NotificationDictionary> notifications, Func<WebHook, string, bool> predicate)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }
            if (notifications == null)
            {
                throw new ArgumentNullException(nameof(notifications));
            }
            if (!notifications.Any())
            {
                return 0;
            }

            // Get the User ID from the User principal
            IWebHookUser user = DependencyResolver.Current.GetUser();
            string userId = await user.GetUserIdAsync(controller.User);

            // Send a notification to registered WebHooks with matching filters
            IWebHookManager manager = DependencyResolver.Current.GetManager();
            return await manager.NotifyAsync(userId, notifications, predicate);
        }

        /// <summary>
        /// Submits a notification to all matching registered WebHooks across all users. To match, the <see cref="WebHook"/> must 
        /// have a filter that matches one or more of the actions provided for the notification.
        /// </summary>
        /// <param name="controller">The <see cref="Controller"/> instance.</param>
        /// <param name="action">The action describing the notification.</param>
        /// <param name="data">Optional additional data to include in the WebHook request.</param>
        /// <returns>The number of <see cref="WebHook"/> instances that were selected and subsequently notified about the actions.</returns>
        public static Task<int> NotifyAllAsync(this Controller controller, string action, object data)
        {
            var notifications = new NotificationDictionary[] { new NotificationDictionary(action, data) };
            return NotifyAllAsync(controller, notifications, predicate: null);
        }

        /// <summary>
        /// Submits a notification to all matching registered WebHooks across all users. To match, the <see cref="WebHook"/> must 
        /// have a filter that matches one or more of the actions provided for the notification.
        /// </summary>
        /// <param name="controller">The <see cref="Controller"/> instance.</param>
        /// <param name="action">The action describing the notification.</param>
        /// <param name="data">Optional additional data to include in the WebHook request.</param>
        /// <param name="predicate">A function to test each <see cref="WebHook"/> to see whether it fulfills the condition. The
        /// predicate is passed the <see cref="WebHook"/> and the user who registered it. If the predicate returns <c>true</c> then
        /// the <see cref="WebHook"/> is included; otherwise it is not.</param>
        /// <returns>The number of <see cref="WebHook"/> instances that were selected and subsequently notified about the actions.</returns>
        public static Task<int> NotifyAllAsync(this Controller controller, string action, object data, Func<WebHook, string, bool> predicate)
        {
            var notifications = new NotificationDictionary[] { new NotificationDictionary(action, data) };
            return NotifyAllAsync(controller, notifications, predicate);
        }

        /// <summary>
        /// Submits a notification to all matching registered WebHooks across all users. To match, the <see cref="WebHook"/> must 
        /// have a filter that matches one or more of the actions provided for the notification.
        /// </summary>
        /// <param name="controller">The <see cref="Controller"/> instance.</param>
        /// <param name="notifications">The set of notifications to include in the WebHook.</param>
        /// <returns>The number of <see cref="WebHook"/> instances that were selected and subsequently notified about the actions.</returns>
        public static Task<int> NotifyAllAsync(this Controller controller, params NotificationDictionary[] notifications)
        {
            return NotifyAllAsync(controller, notifications, predicate: null);
        }

        /// <summary>
        /// Submits a notification to all matching registered WebHooks across all users. To match, the <see cref="WebHook"/> must 
        /// have a filter that matches one or more of the actions provided for the notification.
        /// </summary>
        /// <param name="controller">The <see cref="Controller"/> instance.</param>
        /// <param name="notifications">The set of notifications to include in the WebHook.</param>
        /// <param name="predicate">A function to test each <see cref="WebHook"/> to see whether it fulfills the condition. The
        /// predicate is passed the <see cref="WebHook"/> and the user who registered it. If the predicate returns <c>true</c> then
        /// the <see cref="WebHook"/> is included; otherwise it is not.</param>
        /// <returns>The number of <see cref="WebHook"/> instances that were selected and subsequently notified about the actions.</returns>
        public static async Task<int> NotifyAllAsync(this Controller controller, IEnumerable<NotificationDictionary> notifications, Func<WebHook, string, bool> predicate)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }
            if (notifications == null)
            {
                throw new ArgumentNullException(nameof(notifications));
            }
            if (!notifications.Any())
            {
                return 0;
            }

            // Send a notification to registered WebHooks across all users with matching filters
            IWebHookManager manager = DependencyResolver.Current.GetManager();
            return await manager.NotifyAllAsync(notifications, predicate);
        }
    }
}
