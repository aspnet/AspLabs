// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// <para>
    /// An <see cref="System.Attribute"/> indicating the associated action is a Pusher WebHook endpoint. Specifies the
    /// optional <see cref="WebHookAttribute.Id"/>. Also adds a <see cref="Filters.WebHookReceiverExistsFilter"/> for
    /// the action.
    /// </para>
    /// <para>
    /// The signature of the action should be:
    /// <code>
    /// Task{IActionResult} ActionName(string id, string[] events, TData data)
    /// </code>
    /// or include the subset of parameters required. <c>TData</c> must be compatible with expected requests e.g.
    /// <see cref="Newtonsoft.Json.Linq.JObject"/> or <see cref="PusherNotifications"/>.
    /// </para>
    /// <para>
    /// An example Pusher WebHook URI is '<c>https://{host}/api/webhooks/incoming/pusher/{id}</c>'. See
    /// <see href="https://pusher.com/docs/webhooks"/> for additional details about Pusher WebHook requests.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the application enables CORS in general (see the <c>Microsoft.AspNetCore.Cors</c> package), apply
    /// <c>DisableCorsAttribute</c> to this action. If the application depends on the
    /// <c>Microsoft.AspNetCore.Mvc.ViewFeatures</c> package, apply <c>IgnoreAntiforgeryTokenAttribute</c> to this
    /// action.
    /// </para>
    /// <para>
    /// <see cref="PusherWebHookAttribute"/> should be used at most once per <see cref="WebHookAttribute.Id"/> in a
    /// WebHook application.
    /// </para>
    /// </remarks>
    public class PusherWebHookAttribute : WebHookAttribute
    {
        /// <summary>
        /// Instantiates a new <see cref="PusherWebHookAttribute"/> indicating the associated action is a Pusher
        /// WebHook endpoint.
        /// </summary>
        public PusherWebHookAttribute()
            : base(PusherConstants.ReceiverName)
        {
        }
    }
}
