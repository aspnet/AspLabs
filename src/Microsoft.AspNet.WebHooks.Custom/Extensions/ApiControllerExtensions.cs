// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
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
        /// <param name="actions">One or more actions describing the notification.</param>
        /// <returns>The number of <see cref="WebHook"/> instances that were selected and subsequently notified about the actions.</returns>
        public static Task<int> NotifyAsync(this ApiController controller, params string[] actions)
        {
            return NotifyAsync(controller, actions, data: null);
        }

        /// <summary>
        /// Submits a notification to all matching registered WebHooks. To match, the <see cref="WebHook"/> must be registered by the
        /// current <see cref="ApiController.User"/> and have a filter that matches one or more of the actions provided for the notification.
        /// </summary>
        /// <param name="controller">The <see cref="ApiController"/> instance.</param>
        /// <param name="actions">One or more actions describing the notification.</param>
        /// <param name="data">Optional additional data to include in the WebHook request.</param>
        /// <returns>The number of <see cref="WebHook"/> instances that were selected and subsequently notified about the actions.</returns>
        public static async Task<int> NotifyAsync(this ApiController controller, IEnumerable<string> actions, object data)
        {
            if (controller == null)
            {
                throw new ArgumentNullException("controller");
            }

            // Get the User ID from the User principal
            IWebHookUser user = controller.Configuration.DependencyResolver.GetUser();
            string userId = await user.GetUserIdAsync(controller.User);

            // Send a notification to registered WebHooks with matching filters
            IWebHookManager manager = controller.Configuration.DependencyResolver.GetManager();
            return await manager.NotifyAsync(userId, actions, data);
        }
    }
}
