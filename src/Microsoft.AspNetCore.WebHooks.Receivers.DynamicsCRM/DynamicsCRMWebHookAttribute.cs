// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// <para>
    /// An <see cref="System.Attribute"/> indicating the associated action is a Dynamics CRM WebHook endpoint.
    /// Specifies the optional <see cref="WebHookAttribute.Id"/>. Also adds a
    /// <see cref="Filters.WebHookReceiverExistsFilter"/> for the action.
    /// </para>
    /// <para>
    /// An example Dynamics CRM WebHook URI is
    /// '<c>https://&lt;host&gt;/api/webhooks/incoming/dynamicscrm/{id}?code=83699ec7c1d794c0c780e49a5c72972590571fd8</c>'.
    /// See <see href="http://go.microsoft.com/fwlink/?LinkId=722218"/> for additional details about Dynamics CRM
    /// WebHook requests.
    /// </para>
    /// </summary>
    public class DynamicsCRMWebHookAttribute : WebHookAttribute
    {
        /// <summary>
        /// <para>
        /// Instantiates a new <see cref="DynamicsCRMWebHookAttribute"/> indicating the associated action is a Dynamics
        /// CRM WebHook endpoint.
        /// </para>
        /// <para>The signature of the action should be:
        /// <code>
        /// Task{IActionResult} ActionName(string id, TData data)
        /// </code>
        /// or include the subset of parameters required. <c>TData</c> must be compatible with expected requests e.g.
        /// <see cref="Newtonsoft.Json.Linq.JObject"/>.
        /// </para>
        /// <para>This constructor should usually be used at most once in a WebHook application.</para>
        /// </summary>
        public DynamicsCRMWebHookAttribute()
            : base(DynamicsCRMConstants.ReceiverName)
        {
        }
    }
}