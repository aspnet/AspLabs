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
        /// Initializes support for receiving Stripe WebHooks. Set the '<c>MS_WebHookReceiverSecret_Stripe</c>'
        /// application setting to the signing secret defined in Stripe. The corresponding WebHook URI is of the form
        /// '<c>https://&lt;host&gt;/api/webhooks/incoming/stripe/{id}</c>'. For details about Stripe WebHooks, see
        /// <c>https://stripe.com/docs/webhooks</c>. For details about Stripe signing secrets, see
        /// <see href="https://stripe.com/docs/webhooks#signatures"/>.
        /// </summary>
        /// <param name="config">The current <see cref="HttpConfiguration"/>config.</param>
        public static void InitializeReceiveStripeWebHooks(this HttpConfiguration config)
        {
            WebHooksConfig.Initialize(config);
        }
    }
}
