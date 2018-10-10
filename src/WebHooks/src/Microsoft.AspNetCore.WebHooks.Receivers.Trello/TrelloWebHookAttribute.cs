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
    /// An <see cref="Attribute"/> indicating the associated action is a Trello WebHook endpoint. Specifies the
    /// optional <see cref="WebHookAttribute.Id"/>. Also adds a <see cref="WebHookReceiverExistsFilter"/> and a
    /// <see cref="ModelStateInvalidFilter"/> (unless <see cref="ApiBehaviorOptions.SuppressModelStateInvalidFilter"/>
    /// is <see langword="true"/>) for the action.
    /// </para>
    /// <para>
    /// The signature of the action should be:
    /// <code>
    /// Task{IActionResult} ActionName(string id, string @event, TData data)
    /// </code>
    /// or include the subset of parameters required. <c>@event</c> will always contain the value <c>"change"</c>.
    /// <c>TData</c> must be compatible with expected requests e.g. <see cref="Newtonsoft.Json.Linq.JObject"/>.
    /// </para>
    /// <para>
    /// An example Trello WebHook URI is '<c>https://{host}/api/webhooks/incoming/trello/{id}</c>'. See
    /// <see href="https://developers.trello.com/page/webhooks"/> for additional details about Trello WebHook requests.
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
    /// <see cref="TrelloWebHookAttribute"/> should be used at most once per <see cref="WebHookAttribute.Id"/> in a
    /// WebHook application.
    /// </para>
    /// </remarks>
    public class TrelloWebHookAttribute : WebHookAttribute
    {
        /// <summary>
        /// Instantiates a new <see cref="TrelloWebHookAttribute"/> indicating the associated action is a Trello
        /// WebHook endpoint.
        /// </summary>
        public TrelloWebHookAttribute()
            : base(TrelloConstants.ReceiverName)
        {
        }
    }
}
