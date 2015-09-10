// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using Microsoft.AspNet.WebHooks.Config;

namespace System.Web.Http
{
    /// <summary>
    /// Extension methods for <see cref="HttpConfiguration"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpConfigurationExtensions
    {
        /// <summary>
        /// Initializes support for receiving Pusher WebHooks.
        /// The '<c>MS_WebHookReceiverSecret_Pusher</c>' application setting contains a semicolon separated list of values 
        /// of the form '<c>appKey_appSecret</c>' containing one or more application key/secret pairs defined in Pusher. An example
        /// with two key/secret pairs is '<c>47e5a8cd8f6bb492252a_42fef23870926753d345; ba3af8f38f3be37d476a_9eb6d047bb5465a43cb2</c>'.
        /// The corresponding WebHook URI is of the form '<c>https://&lt;host&gt;/api/webhooks/incoming/pusher/{id}</c>'.
        /// For details about Pusher WebHooks, see <c>https://pusher.com/docs/webhooks</c>.
        /// </summary>
        /// <param name="config">The current <see cref="HttpConfiguration"/>config.</param>
        public static void InitializeReceivePusherWebHooks(this HttpConfiguration config)
        {
            WebHooksConfig.Initialize(config);
        }
    }
}
