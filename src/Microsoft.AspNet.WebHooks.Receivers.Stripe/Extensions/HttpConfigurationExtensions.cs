// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Web.Http.Dependencies;
using Microsoft.AspNet.WebHooks;
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
        /// Initializes support for receiving Stripe WebHooks. 
        /// Set the '<c>MS_WebHookReceiverSecret_Stripe</c>' application setting to the application key defined in Stripe.
        /// The corresponding WebHook URI is of the form '<c>https://&lt;host&gt;/api/webhooks/incoming/stripe/{id}</c>'.
        /// As there is no code embedded in the URI, this mode will cause a follow-up HTTP GET request to Stripe to 
        /// get the actual WebHook data.
        /// For details about Stripe WebHooks, see <c>https://stripe.com/docs/webhooks</c>.
        /// </summary>
        /// <param name="config">The current <see cref="HttpConfiguration"/>config.</param>
        public static void InitializeReceiveStripeWebHooks(this HttpConfiguration config)
        {
            WebHooksConfig.Initialize(config);
        }

        /// <summary>
        /// Initializes support for receiving Stripe WebHooks without any follow-up HTTP GET request to get the WebHook data. 
        /// A sample WebHook URI is '<c>https://&lt;host&gt;/api/webhooks/incoming/stripe/{id}?code=83699ec7c1d794c0c780e49a5c72972590571fd8</c>'.
        /// For security reasons the WebHook URI must be an <c>https</c> URI and contain a 'code' query parameter with the
        /// same value as configured in the '<c>MS_WebHookReceiverSecret_Stripe</c>' application setting.
        /// The 'code' parameter must be between 32 and 128 characters long.
        /// For details about Stripe WebHooks, see <c>https://stripe.com/docs/webhooks</c>.
        /// </summary>
        /// <param name="config">The current <see cref="HttpConfiguration"/>config.</param>
        public static void InitializeReceiveStripeDirectWebHooks(this HttpConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            // Enable direct mode
            IDependencyResolver resolver = config.DependencyResolver;
            SettingsDictionary settings = resolver.GetSettings();
            settings[StripeWebHookReceiver.DirectWebHook] = bool.TrueString;

            WebHooksConfig.Initialize(config);
        }
    }
}
