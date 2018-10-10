// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.WebHooks.Filters;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// <para>
    /// An <see cref="Attribute"/> indicating the associated action is a MailChimp WebHook endpoint. Specifies
    /// the optional <see cref="WebHookAttribute.Id"/>. Also adds a <see cref="WebHookReceiverExistsFilter"/> and a
    /// <see cref="ModelStateInvalidFilter"/> (unless <see cref="ApiBehaviorOptions.SuppressModelStateInvalidFilter"/>
    /// is <see langword="true"/>) for the action.
    /// </para>
    /// <para>
    /// The signature of the action should be:
    /// <code>
    /// Task{IActionResult} ActionName(string id, string @event, TData data)
    /// </code>
    /// or include the subset of parameters required. <c>TData</c> must be compatible with expected requests e.g.
    /// <see cref="Http.IFormCollection"/>.
    /// </para>
    /// <para>
    /// An example MailChimp WebHook URI is
    /// '<c>https://{host}/api/webhooks/incoming/mailchimp/{id}?code=83699ec7c1d794c0c780e49a5c72972590571fd8</c>'.
    /// See <see href="https://developer.mailchimp.com/documentation/mailchimp/guides/about-webhooks/"/> for additional
    /// details about MailChimp WebHook requests.
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
    /// <see cref="MailChimpWebHookAttribute"/> should be used at most once per <see cref="WebHookAttribute.Id"/> in a
    /// WebHook application.
    /// </para>
    /// </remarks>
    public class MailChimpWebHookAttribute : WebHookAttribute
    {
        /// <summary>
        /// Instantiates a new <see cref="MailChimpWebHookAttribute"/> indicating the associated action is a MailChimp
        /// WebHook endpoint.
        /// </summary>
        public MailChimpWebHookAttribute()
            : base(MailChimpConstants.ReceiverName)
        {
        }
    }
}
