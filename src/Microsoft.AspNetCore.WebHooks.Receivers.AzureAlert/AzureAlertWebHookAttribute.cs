// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// <para>
    /// An <see cref="System.Attribute"/> indicating the associated action is an Azure Alert WebHook endpoint.
    /// Specifies the optional <see cref="WebHookAttribute.Id"/>. Also adds a
    /// <see cref="Filters.WebHookReceiverExistsFilter"/> for the action.
    /// </para>
    /// <para>
    /// An example Azure Alert WebHook URI is
    /// '<c>https://&lt;host&gt;/api/webhooks/incoming/azurealert/{id}?code=83699ec7c1d794c0c780e49a5c72972590571fd8</c>'.
    /// See <see href="https://docs.microsoft.com/en-us/azure/monitoring-and-diagnostics/insights-webhooks-alerts"/>
    /// for additional details about Azure Alert WebHook requests.
    /// </para>
    /// </summary>
    public class AzureAlertWebHookAttribute : WebHookAttribute
    {
        /// <summary>
        /// <para>
        /// Instantiates a new <see cref="AzureAlertWebHookAttribute"/> indicating the associated action is an Azure
        /// Alert WebHook endpoint.
        /// </para>
        /// <para>The signature of the action should be:
        /// <code>
        /// Task{IActionResult} ActionName(string id, TData data)
        /// </code>
        /// or include the subset of parameters required. <c>TData</c> must be compatible with expected requests e.g.
        /// <see cref="Newtonsoft.Json.Linq.JObject"/> or <see cref="AzureAlertNotification"/>.
        /// </para>
        /// <para>This constructor should usually be used at most once in a WebHook application.</para>
        /// </summary>
        public AzureAlertWebHookAttribute()
            : base(AzureAlertConstants.ReceiverName)
        {
        }
    }
}
