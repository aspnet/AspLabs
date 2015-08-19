// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using Microsoft.AspNet.WebHooks.Config;

namespace System.Web.Http
{
    /// <summary>
    /// Extension methods for <see cref="HttpConfiguration"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpConfigurationExtensions
    {
        /// <summary>
        /// Initializes support for receiving Salesforce SOAP-based Outbound Messages as a WebHook.
        /// A sample WebHook URI is of the form '<c>https://&lt;host&gt;/api/webhooks/incoming/sfsoap</c>'.
        /// For security reasons, the WebHook URI must be an <c>https</c> URI and the '<c>MS_WebHookReceiverSecret_SalesforceSoap</c>' 
        /// application setting must be configured to the Salesforce Organization ID. This ID can be found at 
        /// <c>http://www.salesforce.com</c> under <c>Setup | Company Profile | Company Information</c>.
        /// For details about Salesforce Outbound Messages, see <c>https://help.salesforce.com/htviewhelpdoc?id=workflow_defining_outbound_messages.htm</c>. 
        /// </summary>
        /// <param name="config">The current <see cref="HttpConfiguration"/>config.</param>
        public static void InitializeReceiveSalesforceWebHooks(this HttpConfiguration config)
        {
            WebHooksConfig.Initialize(config);
        }
    }
}
