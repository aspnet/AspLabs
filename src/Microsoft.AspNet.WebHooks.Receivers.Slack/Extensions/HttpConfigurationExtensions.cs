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
        /// Initializes support for receiving Slack WebHooks.
        /// A sample WebHook URI is of the form '<c>https://&lt;host&gt;/api/webhooks/incoming/slack/{id}</c>'.
        /// For security reasons, the WebHook URI must be an <c>https</c> URI and the WebHook 'token' parameter 
        /// must have the same value as configured in the '<c>MS_WebHookReceiverSecret_Slack</c>' application setting.
        /// For details about Slack WebHooks, see <c>https://api.slack.com/outgoing-webhooks</c>.
        /// </summary>
        /// <param name="config">The current <see cref="HttpConfiguration"/>config.</param>
        public static void InitializeReceiveSlackWebHooks(this HttpConfiguration config)
        {
            WebHooksConfig.Initialize(config);
        }
    }
}
