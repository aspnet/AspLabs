// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// <para>
    /// An <see cref="System.Attribute"/> indicating the associated action is a Salesforce WebHook endpoint. Specifies
    /// the optional <see cref="WebHookAttribute.Id"/>. Also adds a <see cref="Filters.WebHookReceiverExistsFilter"/>
    /// for the action.
    /// </para>
    /// <para>The signature of the action should be:
    /// <code>
    /// Task{IActionResult} ActionName(string id, string @event, TData data)
    /// </code>
    /// or include the subset of parameters required. <c>TData</c> must be compatible with expected requests e.g.
    /// <see cref="System.Xml.Linq.XElement"/> or <see cref="SalesforceNotifications"/>.
    /// </para>
    /// <para>
    /// The '<c>MS_WebHookReceiverSecret_SalesforceSoap</c>' configuration value contains Salesforce Organization IDs.
    /// The Organizational IDs can be found at <see href="http://www.salesforce.com"/> under
    /// <c>Setup | Company Profile | Company Information</c>.
    /// </para>
    /// <para>
    /// An example Salesforce WebHook URI is '<c>https://&lt;host&gt;/api/webhooks/incoming/sfsoap/{id}</c>'.
    /// See <see href="https://go.microsoft.com/fwlink/?linkid=838587"/> for additional details about Salesforce
    /// WebHook requests.
    /// </para>
    /// </summary>
    public class SalesforceWebHookAttribute : WebHookAttribute
    {
        /// <summary>
        /// <para>
        /// Instantiates a new <see cref="SalesforceWebHookAttribute"/> indicating the associated action is a
        /// Salesforce WebHook endpoint.
        /// </para>
        /// <para>This constructor should usually be used at most once in a WebHook application.</para>
        /// <para>
        /// The default route <see cref="Mvc.Routing.IRouteTemplateProvider.Name"/> is <see langword="null"/>.
        /// </para>
        /// </summary>
        public SalesforceWebHookAttribute()
            : base(SalesforceConstants.ReceiverName)
        {
        }
    }
}