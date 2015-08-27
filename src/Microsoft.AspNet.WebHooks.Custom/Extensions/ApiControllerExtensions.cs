// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks;

namespace System.Web.Http
{
    /// <summary>
    /// Various extension methods for the ASP.NET Web API <see cref="ApiController"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ApiControllerExtensions
    {
        /// <summary>
        /// Submits a notification to all matching registered WebHooks. To match, the <see cref="WebHook"/> must be registered by the
        /// current <see cref="ApiController.User"/> and have a filter that matches one or more of the actions provided for the notification.
        /// </summary>
        /// <param name="controller">The <see cref="ApiController"/> instance.</param>
        /// <param name="action">The action describing the notification.</param>
        /// <param name="data">Optional additional data to include in the WebHook request.</param>
        /// <returns>The number of <see cref="WebHook"/> instances that were selected and subsequently notified about the actions.</returns>
        public static Task<int> NotifyAsync(this ApiController controller, string action, object data)
        {
            NotificationDictionary notification = new NotificationDictionary(action, data);
            return NotifyAsync(controller, notification);
        }

        /// <summary>
        /// Submits a notification to all matching registered WebHooks. To match, the <see cref="WebHook"/> must be registered by the
        /// current <see cref="ApiController.User"/> and have a filter that matches one or more of the actions provided for the notification.
        /// </summary>
        /// <param name="controller">The <see cref="ApiController"/> instance.</param>
        /// <param name="notifications">The set of notifications to include in the WebHook.</param>
        /// <returns>The number of <see cref="WebHook"/> instances that were selected and subsequently notified about the actions.</returns>
        public static async Task<int> NotifyAsync(this ApiController controller, params NotificationDictionary[] notifications)
        {
            if (controller == null)
            {
                throw new ArgumentNullException("controller");
            }
            if (notifications == null)
            {
                throw new ArgumentNullException("notifications");
            }
            if (notifications.Length == 0)
            {
                return 0;
            }

            // Get the User ID from the User principal
            IWebHookUser user = controller.Configuration.DependencyResolver.GetUser();
            string userId = await user.GetUserIdAsync(controller.User);

            // Send a notification to registered WebHooks with matching filters
            IWebHookManager manager = controller.Configuration.DependencyResolver.GetManager();
            return await manager.NotifyAsync(userId, notifications);
        }
    }
}
