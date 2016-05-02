// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks.Payloads;
using Microsoft.AspNet.WebHooks.Properties;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides a base <see cref="IWebHookHandler" /> implementation which can be used to for handling Visual Studio Team Services WebHook 
    /// using strongly-typed payloads. For details about MyGet WebHooks, see <c>https://www.visualstudio.com/en-us/get-started/integrate/service-hooks/webhooks-and-vso-vs</c>.
    /// </summary>
    public abstract class VstsWebHookHandlerBase : WebHookHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VstsWebHookHandlerBase"/> class.
        /// </summary>
        protected VstsWebHookHandlerBase()
        {
            this.Receiver = VstsWebHookReceiver.ReceiverName;
        }

        /// <inheritdoc />
        public override Task ExecuteAsync(string receiver, WebHookHandlerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            string action = context.Actions.First();
            JObject data = context.GetDataOrDefault<JObject>();

            // map eventType to corresponding payload
            switch (action)
            {
                case "workitem.updated": return ExecuteAsync(context, data.ToObject<WorkItemUpdatedPayload>());
                case "workitem.restored": return ExecuteAsync(context, data.ToObject<WorkItemRestoredPayload>());
                case "workitem.deleted": return ExecuteAsync(context, data.ToObject<WorkItemDeletedPayload>());
                case "workitem.created": return ExecuteAsync(context, data.ToObject<WorkItemCreatedPayload>());
                case "workitem.commented": return ExecuteAsync(context, data.ToObject<WorkItemCommentedOnPayload>());
                case "message.posted": return ExecuteAsync(context, data.ToObject<TeamRoomMessagePostedPayload>());
                case "tfvc.checkin": return ExecuteAsync(context, data.ToObject<CodeCheckedInPayload>());
                case "build.complete": return ExecuteAsync(context, data.ToObject<BuildCompletedPayload>());
                default:
                    string msg = string.Format(CultureInfo.CurrentCulture, VstsReceiverResources.Handler_NonMappedEventType, action);
                    context.RequestContext.Configuration.DependencyResolver.GetLogger().Warn(msg);
                    return ExecuteAsync(context, data);
            }
        }

        /// <summary>
        /// Executes the incoming WebHook request for event '<c>workitem.updated</c>'.
        /// </summary>
        /// <param name="context">Provides context for the <see cref="IWebHookHandler"/> for further processing the incoming WebHook.</param>
        /// <param name="payload">Strong-typed WebHook payload.</param>
        public virtual Task ExecuteAsync(WebHookHandlerContext context, WorkItemUpdatedPayload payload)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Executes the incoming WebHook request for event '<c>workitem.restored</c>'.
        /// </summary>
        /// <param name="context">Provides context for the <see cref="IWebHookHandler"/> for further processing the incoming WebHook.</param>
        /// <param name="payload">Strong-typed WebHook payload.</param>
        public virtual Task ExecuteAsync(WebHookHandlerContext context, WorkItemRestoredPayload payload)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Executes the incoming WebHook request for event '<c>workitem.deleted</c>'.
        /// </summary>
        /// <param name="context">Provides context for the <see cref="IWebHookHandler"/> for further processing the incoming WebHook.</param>
        /// <param name="payload">Strong-typed WebHook payload.</param>
        public virtual Task ExecuteAsync(WebHookHandlerContext context, WorkItemDeletedPayload payload)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Executes the incoming WebHook request for event '<c>workitem.created</c>'.
        /// </summary>
        /// <param name="context">Provides context for the <see cref="IWebHookHandler"/> for further processing the incoming WebHook.</param>
        /// <param name="payload">Strong-typed WebHook payload.</param>
        public virtual Task ExecuteAsync(WebHookHandlerContext context, WorkItemCreatedPayload payload)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Executes the incoming WebHook request for event '<c>workitem.commented</c>'.
        /// </summary>
        /// <param name="context">Provides context for the <see cref="IWebHookHandler"/> for further processing the incoming WebHook.</param>
        /// <param name="payload">Strong-typed WebHook payload.</param>
        public virtual Task ExecuteAsync(WebHookHandlerContext context, WorkItemCommentedOnPayload payload)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Executes the incoming WebHook request for event '<c>message.posted</c>'.
        /// </summary>
        /// <param name="context">Provides context for the <see cref="IWebHookHandler"/> for further processing the incoming WebHook.</param>
        /// <param name="payload">Strong-typed WebHook payload.</param>
        public virtual Task ExecuteAsync(WebHookHandlerContext context, TeamRoomMessagePostedPayload payload)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Executes the incoming WebHook request for event '<c>tfvc.checkin</c>'.
        /// </summary>
        /// <param name="context">Provides context for the <see cref="IWebHookHandler"/> for further processing the incoming WebHook.</param>
        /// <param name="payload">Strong-typed WebHook payload.</param>
        public virtual Task ExecuteAsync(WebHookHandlerContext context, CodeCheckedInPayload payload)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Executes the incoming WebHook request for event '<c>build.complete</c>'.
        /// </summary>
        /// <param name="context">Provides context for the <see cref="IWebHookHandler"/> for further processing the incoming WebHook.</param>
        /// <param name="payload">Strong-typed WebHook payload.</param>
        public virtual Task ExecuteAsync(WebHookHandlerContext context, BuildCompletedPayload payload)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Executes the incoming WebHook request for unknown event.
        /// </summary>
        /// <param name="context">Provides context for the <see cref="IWebHookHandler"/> for further processing the incoming WebHook.</param>
        /// <param name="payload">JSON payload.</param>
        public virtual Task ExecuteAsync(WebHookHandlerContext context, JObject payload)
        {
            return Task.FromResult(true);
        }
    }
}
