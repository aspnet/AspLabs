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
        /// Initializes support for receiving Zendesk WebHooks.
        /// A sample WebHook URI is '<c>https://&lt;host&gt;/api/webhooks/incoming/zendesk/{id}?code=83699ec7c1d794c0c780e49a5c72972590571fd8</c>'.
        /// For security reasons the WebHook URI must be an <c>https</c> URI and contain a 'code' query parameter with the
        /// same value as configured in the '<c>MS_WebHookReceiverSecret_Zendesk</c>' application setting.
        /// The 'code' parameter must be between 32 and 128 characters long.
        /// For details about Zendesk WebHooks, see <c>https://developer.zendesk.com/embeddables/docs/ios/push_notifications_webhook</c>.
        /// For complete details about Zendesk APIs, see <c>https://developer.zendesk.com/rest_api/docs/core/introduction</c>.
        /// </summary>
        /// <param name="config">The current <see cref="HttpConfiguration"/>config.</param>
        public static void InitializeReceiveZendeskWebHooks(this HttpConfiguration config)
        {
            WebHooksConfig.Initialize(config);
        }
    }
}