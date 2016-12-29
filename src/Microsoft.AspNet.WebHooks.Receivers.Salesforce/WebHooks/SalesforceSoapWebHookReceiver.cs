// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
    /// A sample WebHook URI is of the form '<c>https://&lt;host&gt;/api/webhooks/incoming/sfsoap/{id}</c>'.
    /// For security reasons, the WebHook URI must be an <c>https</c> URI and the '<c>MS_WebHookReceiverSecret_SalesforceSoap</c>' 
    /// application setting must be configured to the Salesforce Organization IDs. The Organizational IDs can be found at 
    /// <c>http://www.salesforce.com</c> under <c>Setup | Company Profile | Company Information</c>.
    /// For details about Salesforce Outbound Messages, see <c>https://go.microsoft.com/fwlink/?linkid=838587</c>. 
    /// </summary>
    public class SalesforceSoapWebHookReceiver : WebHookReceiver
    {
        internal const string RecName = "sfsoap";
        internal const string ReceiverConfigName = "SalesforceSoap";

        /// <summary>
        /// Gets the receiver name for this receiver.
        /// </summary>
        public static string ReceiverName
        {
            get { return RecName; }
        }

        /// <inheritdoc />
        public override string Name
        {
            get { return RecName; }
        }

        /// <inheritdoc />
        public override async Task<HttpResponseMessage> ReceiveAsync(string id, HttpRequestContext context, HttpRequestMessage request)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.Method == HttpMethod.Post)
            {
                EnsureSecureConnection(request);

                // Read the request entity body
                XElement data = await ReadAsXmlAsync(request);
                SalesforceNotifications notifications = new SalesforceNotifications(data);

                // Ensure that the organization ID matches the expected value.
                string orgId = GetShortOrgId(notifications.OrganizationId);
                string secret = await GetReceiverConfig(request, ReceiverConfigName, id, 15, 18);
                string secretKey = GetShortOrgId(secret);
                if (!WebHookReceiver.SecretEqual(orgId, secretKey))
                {
                    string msg = string.Format(CultureInfo.CurrentCulture, SalesforceReceiverResources.Receiver_BadValue, "OrganizationId");
                    context.Configuration.DependencyResolver.GetLogger().Error(msg);
                    string fault = string.Format(CultureInfo.InvariantCulture, ReadResource("Microsoft.AspNet.WebHooks.Messages.FaultResponse.xml"), msg);
                    HttpResponseMessage invalidId = GetXmlResponse(request, HttpStatusCode.BadRequest, fault);
                    return invalidId;
                }

                // Get the action
                string action = notifications.ActionId;
                if (string.IsNullOrEmpty(action))
                {
                    string msg = string.Format(CultureInfo.CurrentCulture, SalesforceReceiverResources.Receiver_BadBody, "ActionId");
                    context.Configuration.DependencyResolver.GetLogger().Error(msg);
                    string fault = string.Format(CultureInfo.InvariantCulture, ReadResource("Microsoft.AspNet.WebHooks.Messages.FaultResponse.xml"), msg);
                    HttpResponseMessage badType = GetXmlResponse(request, HttpStatusCode.BadRequest, fault);
                    return badType;
                }

                // Call registered handlers
                HttpResponseMessage response = await ExecuteWebHookAsync(id, context, request, new string[] { action }, notifications);

                // Add SOAP response if not already present
                if (response == null || response.Content == null || !response.Content.IsXml())
                {
                    string ack = ReadResource("Microsoft.AspNet.WebHooks.Messages.NotificationResponse.xml");
                    response = GetXmlResponse(request, HttpStatusCode.OK, ack);
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

        internal static HttpResponseMessage GetXmlResponse(HttpRequestMessage request, HttpStatusCode statusCode, string msg)
        {
            HttpResponseMessage response = request.CreateResponse(statusCode);
            response.Content = new StringContent(msg, Encoding.UTF8, "application/xml");
            return response;
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
