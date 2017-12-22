// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// <para>
    /// An <see cref="System.Attribute"/> indicating the associated action is a Dropbox WebHook endpoint. Specifies the
    /// optional <see cref="WebHookAttribute.Id"/>. Also adds a <see cref="Filters.WebHookReceiverExistsFilter"/> for
    /// the action.
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
    /// An example Dropbox WebHook URI is '<c>https://{host}/api/webhooks/incoming/dropbox/{id}</c>'. See
    /// <see href="https://www.dropbox.com/developers/webhooks/docs"/> for additional details about Dropbox WebHook
    /// requests.
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
    /// <see cref="DropboxWebHookAttribute"/> should be used at most once per <see cref="WebHookAttribute.Id"/> in a
    /// WebHook application.
    /// </para>
    /// </remarks>
    public class DropboxWebHookAttribute : WebHookAttribute
    {
        /// <summary>
        /// Instantiates a new <see cref="DropboxWebHookAttribute"/> indicating the associated action is a Dropbox
        /// WebHook endpoint.
        /// </summary>
        public DropboxWebHookAttribute()
            : base(DropboxConstants.ReceiverName)
        {
        }
    }
}
