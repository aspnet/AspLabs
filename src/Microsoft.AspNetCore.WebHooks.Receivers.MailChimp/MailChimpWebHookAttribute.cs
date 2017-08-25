// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// <para>
    /// An <see cref="System.Attribute"/> indicating the associated action is a MailChimp WebHook endpoint. Specifies
    /// the optional <see cref="WebHookAttribute.Id"/>. Also adds a <see cref="Filters.WebHookReceiverExistsFilter"/>
    /// for the action.
    /// </para>
    /// <para>
    /// An example MailChimp WebHook URI is
    /// '<c>https://&lt;host&gt;/api/webhooks/incoming/mailchimp/{id}?code=83699ec7c1d794c0c780e49a5c72972590571fd8</c>'.
    /// See <see href="http://developer.mailchimp.com/documentation/mailchimp/guides/about-webhooks/"/> for additional
    /// details about MailChimp WebHook requests.
    /// </para>
    /// </summary>
    public class MailChimpWebHookAttribute : WebHookAttribute
    {
        /// <summary>
        /// <para>
        /// Instantiates a new <see cref="MailChimpWebHookAttribute"/> indicating the associated action is a MailChimp
        /// WebHook endpoint.
        /// </para>
        /// <para>The signature of the action should be:
        /// <code>
        /// Task{IActionResult} ActionName(string id, TData data)
        /// </code>
        /// or include the subset of parameters required. <c>TData</c> must be compatible with expected requests e.g.
        /// <see cref="Http.IFormCollection"/>.
        /// </para>
        /// <para>This constructor should usually be used at most once in a WebHook application.</para>
        /// <para>
        /// The default route <see cref="Mvc.Routing.IRouteTemplateProvider.Name"/> is <see langword="null"/>.
        /// </para>
        /// </summary>
        public MailChimpWebHookAttribute()
            : base(MailChimpConstants.ReceiverName)
        {
        }
    }
}