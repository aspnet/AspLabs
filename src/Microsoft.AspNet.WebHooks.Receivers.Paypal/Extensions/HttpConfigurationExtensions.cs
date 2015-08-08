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
        /// Initializes support for receiving Paypal WebHooks using the Paypal .NET SDK, see <c>https://www.nuget.org/packages/PayPal</c>.
        /// Configure the Paypal WebHook settings using the web.config file as described in <c>https://github.com/paypal/PayPal-NET-SDK/wiki/Webhook-Event-Validation</c>. 
        /// The corresponding WebHook URI is of the form '<c>https://&lt;host&gt;/api/webhooks/incoming/paypal</c>'.
        /// For details about Paypal WebHooks, see <c>https://developer.paypal.com/webapps/developer/docs/integration/direct/rest-webhooks-overview/</c>.
        /// </summary>
        /// <param name="config">The current <see cref="HttpConfiguration"/>config.</param>
        public static void InitializeReceivePaypalWebHooks(this HttpConfiguration config)
        {
            WebHooksConfig.Initialize(config);
        }
    }
}
