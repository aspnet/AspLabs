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
    /// The '<c>MS_WebHookReceiverSecret_Pusher</c>' configuration value contains a semicolon-separated list of values
    /// of the form '<c>appKey_secretKey</c>'. The underscore-separated strings are application key / secret key pairs
    /// defined in Pusher. An example configuration value containing two application key / secret key pairs is
    /// '<c>47e5a8cd8f6bb492252a_42fef23870926753d345; ba3af8f38f3be37d476a_9eb6d047bb5465a43cb2</c>'. An example
    /// configuration value containing a default with two application key / secret key pairs and an addition id with
    /// one pair is:
    /// '<c>47e5a8cd8f6bb492252a_42fef23870926753d345; ba3af8f38f3be37d476a_9eb6d047bb5465a43cb2, id1=39edd0bdb9834a588e98_16e92a1c11b6b2f43df7</c>'
    /// </para>
    /// <para>
    /// An example Pusher WebHook URI is '<c>https://&lt;host&gt;/api/webhooks/incoming/pusher/{id}</c>'. See
    /// <see href="https://pusher.com/docs/webhooks"/> for additional details about Pusher WebHook requests.
    /// </para>
    /// </summary>
    public class PusherWebHookAttribute : WebHookAttribute
    {
        /// <summary>
        /// <para>
        /// Instantiates a new <see cref="PusherWebHookAttribute"/> indicating the associated action is a Pusher
        /// WebHook endpoint.
        /// </para>
        /// <para>The signature of the action should be:
        /// <code>
        /// Task{IActionResult} ActionName(string id, TData data)
        /// </code>
        /// or include the subset of parameters required. <c>TData</c> must be compatible with expected requests e.g.
        /// <see cref="Newtonsoft.Json.Linq.JObject"/> or <see cref="PusherNotifications"/>.
        /// </para>
        /// <para>This constructor should usually be used at most once in a WebHook application.</para>
        /// </summary>
        public PusherWebHookAttribute()
            : base(PusherConstants.ReceiverName)
        {
        }
    }
}
