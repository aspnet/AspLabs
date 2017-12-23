// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks.Payloads;
using Microsoft.AspNet.WebHooks.Properties;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides a base <see cref="IWebHookHandler" /> implementation which can be used to for handling MyGet WebHook using strongly-typed
    /// payloads. For details about MyGet WebHooks, see <c>http://docs.myget.org/docs/reference/webhooks</c>.
    /// </summary>
    public abstract class MyGetWebHookHandlerBase : WebHookHandler
    {
        private const string PayloadPropertyName = "Payload";

        /// <summary>
        /// Initializes a new instance of the <see cref="MyGetWebHookHandlerBase"/> class.
        /// </summary>
        protected MyGetWebHookHandlerBase()
        {
            Receiver = MyGetWebHookReceiver.ReceiverName;
        }

        /// <inheritdoc />
        public override Task ExecuteAsync(string receiver, WebHookHandlerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var action = context.Actions.First();
            var data = context.GetDataOrDefault<JObject>();

            // Check if a payload is available
            if (data == null || !data.TryGetValue(PayloadPropertyName, out var payload))
            {
                var message = string.Format(CultureInfo.CurrentCulture, MyGetReceiverResources.Receiver_NoPayload, PayloadPropertyName);
                context.RequestContext.Configuration.DependencyResolver.GetLogger().Error(message);
                context.Response = context.Request.CreateErrorResponse(HttpStatusCode.BadRequest, message);
                return Task.FromResult(true);
            }

            // Cast to correct payload type
            switch (action)
            {
                case "PackageAddedWebHookEventPayloadV1": return ExecuteAsync(receiver, context, payload.ToObject<PackageAddedPayload>());
                case "PackageDeletedWebHookEventPayloadV1": return ExecuteAsync(receiver, context, payload.ToObject<PackageDeletedPayload>());
                case "PackageListedWebHookEventPayloadV1": return ExecuteAsync(receiver, context, payload.ToObject<PackageListedPayload>());
                case "PackagePinnedWebHookEventPayloadV1": return ExecuteAsync(receiver, context, payload.ToObject<PackagePinnedPayload>());
                case "PackagePushedWebHookEventPayloadV1": return ExecuteAsync(receiver, context, payload.ToObject<PackagePushedPayload>());
                case "BuildQueuedWebHookEventPayloadV1": return ExecuteAsync(receiver, context, payload.ToObject<BuildQueuedPayload>());
                case "BuildStartedWebHookEventPayloadV1": return ExecuteAsync(receiver, context, payload.ToObject<BuildStartedPayload>());
                case "BuildFinishedWebHookEventPayloadV1": return ExecuteAsync(receiver, context, payload.ToObject<BuildFinishedPayload>());
            }

            return ExecuteUnknownPayloadAsync(receiver, context, (JObject)payload);
        }

        /// <summary>
        /// Executes the incoming WebHook request.
        /// </summary>
        /// <param name="receiver">The name of the <see cref="IWebHookReceiver"/> which processed the incoming WebHook. The
        /// receiver can for example be <c>dropbox</c> or <c>github</c>.</param>
        /// <param name="context">Provides context for the <see cref="IWebHookHandler"/> for further processing the incoming WebHook.</param>
        /// <param name="payload">Strong-typed WebHook payload.</param>
        public virtual Task ExecuteAsync(string receiver, WebHookHandlerContext context, PackageAddedPayload payload)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Executes the incoming WebHook request.
        /// </summary>
        /// <param name="receiver">The name of the <see cref="IWebHookReceiver"/> which processed the incoming WebHook. The
        /// receiver can for example be <c>dropbox</c> or <c>github</c>.</param>
        /// <param name="context">Provides context for the <see cref="IWebHookHandler"/> for further processing the incoming WebHook.</param>
        /// <param name="payload">Strong-typed WebHook payload.</param>
        public virtual Task ExecuteAsync(string receiver, WebHookHandlerContext context, PackageDeletedPayload payload)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Executes the incoming WebHook request.
        /// </summary>
        /// <param name="receiver">The name of the <see cref="IWebHookReceiver"/> which processed the incoming WebHook. The
        /// receiver can for example be <c>dropbox</c> or <c>github</c>.</param>
        /// <param name="context">Provides context for the <see cref="IWebHookHandler"/> for further processing the incoming WebHook.</param>
        /// <param name="payload">Strong-typed WebHook payload.</param>
        public virtual Task ExecuteAsync(string receiver, WebHookHandlerContext context, PackageListedPayload payload)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Executes the incoming WebHook request.
        /// </summary>
        /// <param name="receiver">The name of the <see cref="IWebHookReceiver"/> which processed the incoming WebHook. The
        /// receiver can for example be <c>dropbox</c> or <c>github</c>.</param>
        /// <param name="context">Provides context for the <see cref="IWebHookHandler"/> for further processing the incoming WebHook.</param>
        /// <param name="payload">Strong-typed WebHook payload.</param>
        public virtual Task ExecuteAsync(string receiver, WebHookHandlerContext context, PackagePinnedPayload payload)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Executes the incoming WebHook request.
        /// </summary>
        /// <param name="receiver">The name of the <see cref="IWebHookReceiver"/> which processed the incoming WebHook. The
        /// receiver can for example be <c>dropbox</c> or <c>github</c>.</param>
        /// <param name="context">Provides context for the <see cref="IWebHookHandler"/> for further processing the incoming WebHook.</param>
        /// <param name="payload">Strong-typed WebHook payload.</param>
        public virtual Task ExecuteAsync(string receiver, WebHookHandlerContext context, PackagePushedPayload payload)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Executes the incoming WebHook request.
        /// </summary>
        /// <param name="receiver">The name of the <see cref="IWebHookReceiver"/> which processed the incoming WebHook. The
        /// receiver can for example be <c>dropbox</c> or <c>github</c>.</param>
        /// <param name="context">Provides context for the <see cref="IWebHookHandler"/> for further processing the incoming WebHook.</param>
        /// <param name="payload">Strong-typed WebHook payload.</param>
        public virtual Task ExecuteAsync(string receiver, WebHookHandlerContext context, BuildQueuedPayload payload)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Executes the incoming WebHook request.
        /// </summary>
        /// <param name="receiver">The name of the <see cref="IWebHookReceiver"/> which processed the incoming WebHook. The
        /// receiver can for example be <c>dropbox</c> or <c>github</c>.</param>
        /// <param name="context">Provides context for the <see cref="IWebHookHandler"/> for further processing the incoming WebHook.</param>
        /// <param name="payload">Strong-typed WebHook payload.</param>
        public virtual Task ExecuteAsync(string receiver, WebHookHandlerContext context, BuildStartedPayload payload)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Executes the incoming WebHook request.
        /// </summary>
        /// <param name="receiver">The name of the <see cref="IWebHookReceiver"/> which processed the incoming WebHook. The
        /// receiver can for example be <c>dropbox</c> or <c>github</c>.</param>
        /// <param name="context">Provides context for the <see cref="IWebHookHandler"/> for further processing the incoming WebHook.</param>
        /// <param name="payload">Strong-typed WebHook payload.</param>
        public virtual Task ExecuteAsync(string receiver, WebHookHandlerContext context, BuildFinishedPayload payload)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Executes the incoming WebHook request for an unknown payload.
        /// </summary>
        /// <param name="receiver">The name of the <see cref="IWebHookReceiver"/> which processed the incoming WebHook. The
        /// receiver can for example be <c>dropbox</c> or <c>github</c>.</param>
        /// <param name="context">Provides context for the <see cref="IWebHookHandler"/> for further processing the incoming WebHook.</param>
        /// <param name="payload">Strong-typed WebHook payload.</param>
        public virtual Task ExecuteUnknownPayloadAsync(string receiver, WebHookHandlerContext context, JObject payload)
        {
            return Task.FromResult(true);
        }
    }
}
