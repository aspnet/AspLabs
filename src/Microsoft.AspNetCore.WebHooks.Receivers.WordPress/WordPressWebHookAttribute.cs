// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// <para>
    /// An <see cref="System.Attribute"/> indicating the associated action is a WordPress WebHook endpoint.
    /// Specifies the optional <see cref="WebHookAttribute.Id"/>. Also adds a
    /// <see cref="Filters.WebHookReceiverExistsFilter"/> for the action.
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
    /// An example WordPress WebHook URI is
    /// '<c>https://{host}/api/webhooks/incoming/wordpress/{id}?code=83699ec7c1d794c0c780e49a5c72972590571fd8</c>'.
    /// See <see href="https://en.support.wordpress.com/webhooks/"/> for additional details about WordPress WebHook
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
    /// <see cref="WordPressWebHookAttribute"/> should be used at most once per <see cref="WebHookAttribute.Id"/> in a
    /// WebHook application.
    /// </para>
    /// </remarks>
    public class WordPressWebHookAttribute : WebHookAttribute
    {
        /// <summary>
        /// Instantiates a new <see cref="WordPressWebHookAttribute"/> indicating the associated action is a WordPress
        /// Alert WebHook endpoint.
        /// </summary>
        public WordPressWebHookAttribute()
            : base(WordPressConstants.ReceiverName)
        {
        }
    }
}
