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
    /// An <see cref="Attribute"/> indicating the associated action is a Salesforce WebHook endpoint. Specifies the
    /// optional <see cref="WebHookAttribute.Id"/>. Also adds a <see cref="WebHookReceiverExistsFilter"/> and a
    /// <see cref="ModelStateInvalidFilter"/> (unless <see cref="ApiBehaviorOptions.SuppressModelStateInvalidFilter"/>
    /// is <see langword="true"/>) for the action.
    /// </para>
    /// <para>
    /// The signature of the action should be:
    /// <code>
    /// Task{IActionResult} ActionName(string id, string @event, TData data, [FromServices] ISalesforceResultCreator resultCreator)
    /// </code>
    /// or include the subset of parameters required. <c>TData</c> must be compatible with expected requests e.g.
    /// <see cref="System.Xml.Linq.XElement"/> or <see cref="SalesforceNotifications"/>. The
    /// <see cref="ISalesforceResultCreator"/> helps to create SOAP responses.
    /// </para>
    /// <para>
    /// An example Salesforce WebHook URI is '<c>https://{host}/api/webhooks/incoming/salesforce/{id}</c>'.
    /// See <see href="https://go.microsoft.com/fwlink/?linkid=838587"/> for additional details about Salesforce
    /// WebHook requests.
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
    /// <see cref="SalesforceWebHookAttribute"/> should be used at most once per <see cref="WebHookAttribute.Id"/> in a
    /// WebHook application.
    /// </para>
    /// </remarks>
    public class SalesforceWebHookAttribute : WebHookAttribute
    {
        /// <summary>
        /// Instantiates a new <see cref="SalesforceWebHookAttribute"/> indicating the associated action is a
        /// Salesforce WebHook endpoint.
        /// </summary>
        public SalesforceWebHookAttribute()
            : base(SalesforceConstants.ReceiverName)
        {
        }
    }
}
