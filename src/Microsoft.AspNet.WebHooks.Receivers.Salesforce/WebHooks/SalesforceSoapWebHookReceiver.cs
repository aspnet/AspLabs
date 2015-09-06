// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Xml.Linq;
using Microsoft.AspNet.WebHooks.Properties;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides an <see cref="IWebHookReceiver"/> implementation which supports Salesforce SOAP-based Outbound Messages as a WebHook.
    /// A sample WebHook URI is of the form '<c>https://&lt;host&gt;/api/webhooks/incoming/sfsoap</c>'.
    /// For security reasons, the WebHook URI must be an <c>https</c> URI and the '<c>MS_WebHookReceiverSecret_SalesforceSoap</c>' 
    /// application setting must be configured to the Salesforce Organization ID. This ID can be found at 
    /// <c>http://www.salesforce.com</c> under <c>Setup | Company Profile | Company Information</c>.
    /// For details about Salesforce Outbound Messages, see <c>https://help.salesforce.com/htviewhelpdoc?id=workflow_defining_outbound_messages.htm</c>. 
    /// </summary>
    public class SalesforceSoapWebHookReceiver : WebHookReceiver
    {
        internal const string SecretKey = "MS_WebHookReceiverSecret_SalesforceSoap";

        private static readonly string[] ReceiverNames = new string[] { "sfsoap" };

        /// <inheritdoc />
        public override IEnumerable<string> Names
        {
            get { return ReceiverNames; }
        }

        /// <inheritdoc />
        public override async Task<HttpResponseMessage> ReceiveAsync(string receiver, HttpRequestContext context, HttpRequestMessage request)
        {
            if (receiver == null)
            {
                throw new ArgumentNullException("receiver");
            }
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (request.Method == HttpMethod.Post)
            {
                EnsureSecureConnection(request);

                // Read the request entity body
                XElement data = await this.ReadBodyAsXmlAsync(request);
                SalesforceNotifications notifications = new SalesforceNotifications(data);

                // Ensure that the organization ID matches the expected value.
                string orgId = GetShortOrgId(notifications.OrganizationId);
                string secretKey = GetShortOrgId(GetWebHookSecret(request, SecretKey, 15, 18));
                if (!WebHookReceiver.SecretEqual(orgId, secretKey))
                {
                    string msg = string.Format(CultureInfo.CurrentCulture, SalesforceReceiverResources.Receiver_BadValue, "OrganizationId");
                    context.Configuration.DependencyResolver.GetLogger().Error(msg);
                    HttpResponseMessage invalidId = request.CreateErrorResponse(HttpStatusCode.BadRequest, msg);
                    return invalidId;
                }

                // Get the action
                string action = notifications.ActionId;
                if (string.IsNullOrEmpty(action))
                {
                    string msg = string.Format(CultureInfo.CurrentCulture, SalesforceReceiverResources.Receiver_BadBody, "ActionId");
                    context.Configuration.DependencyResolver.GetLogger().Error(msg);
                    HttpResponseMessage badType = request.CreateErrorResponse(HttpStatusCode.BadRequest, msg);
                    return badType;
                }

                // Call registered handlers
                HttpResponseMessage response = await ExecuteWebHookAsync(receiver, context, request, new string[] { action }, notifications);

                // Add SOAP response if not already present
                if (response == null || response.Content == null || !response.Content.IsXml())
                {
                    response = request.CreateResponse();
                    string ack = ReadResource("Microsoft.AspNet.WebHooks.Messages.NotificationResponse.xml");
                    response.Content = new StringContent(ack, Encoding.UTF8, "application/xml");
                }
                return response;
            }
            else
            {
                return CreateBadMethodResponse(request);
            }
        }

        internal static string GetShortOrgId(string fullOrgId)
        {
            if (fullOrgId != null && fullOrgId.Length == 18)
            {
                return fullOrgId.Substring(0, 15);
            }
            return fullOrgId;
        }

        internal static string ReadResource(string name)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            Stream content = asm.GetManifestResourceStream(name);
            if (content == null)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, SalesforceReceiverResources.EmbeddedResources_Unknown, name);
                throw new InvalidOperationException(msg);
            }

            using (StreamReader reader = new StreamReader(content))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
