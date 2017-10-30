// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// <para>
    /// An <see cref="System.Attribute"/> indicating the associated action is a Stripe WebHook endpoint. Specifies the
    /// optional <see cref="WebHookAttribute.Id"/>. Also adds a <see cref="Filters.WebHookReceiverExistsFilter"/> for
    /// the action.
    /// </para>
    /// <para>The signature of the action should be:
    /// <code>
    /// Task{IActionResult} ActionName(string id, string @event, TData data)
    /// </code>
    /// or include the subset of parameters required. <c>TData</c> must be compatible with expected requests e.g.
    /// <see cref="Newtonsoft.Json.Linq.JObject"/> or <see cref="StripeEvent"/>.
    /// </para>
    /// <para>
    /// An example Stripe WebHook URI is '<c>https://&lt;host&gt;/api/webhooks/incoming/stripe/{id}</c>' or
    /// '<c>https://&lt;host&gt;/api/webhooks/incoming/stripe/{id}?code=83699ec7c1d794c0c780e49a5c72972590571fd8</c>'
    /// when the <c>MS_WebHookStripeDirect</c> configuration value is <see langword="true"/>.
    /// </para>
    /// <para>
    /// See <see href="https://stripe.com/docs/webhooks"/> for additional details about Stripe WebHook requests.
    /// See <see href="https://stripe.com/docs/connect/webhooks"/> for additional details about Stripe Connect WebHook
    /// requests. And, see <see href="https://stripe.com/docs/api/dotnet#events"/> for additional details about Stripe
    /// WebHook request payloads.
    /// </para>
    /// </summary>
    public class StripeWebHookAttribute : WebHookAttribute
    {
        /// <summary>
        /// <para>
        /// Instantiates a new <see cref="StripeWebHookAttribute"/> indicating the associated action is a
        /// Stripe WebHook endpoint.
        /// </para>
        /// <para>This constructor should usually be used at most once in a WebHook application.</para>
        /// </summary>
        public StripeWebHookAttribute()
            : base(StripeConstants.ReceiverName)
        {
        }
    }
}
