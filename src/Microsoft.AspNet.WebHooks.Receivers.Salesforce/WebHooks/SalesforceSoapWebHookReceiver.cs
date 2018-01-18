// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using Microsoft.AspNet.WebHooks.Properties;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides an <see cref="IWebHookReceiver"/> implementation which supports Salesforce SOAP-based Outbound Messages as a WebHook.
    /// A sample WebHook URI is of the form '<c>https://&lt;host&gt;/api/webhooks/incoming/sfsoap/{id}</c>'.
    /// For security reasons, the WebHook URI must be an <c>https</c> URI and the '<c>MS_WebHookReceiverSecret_SalesforceSoap</c>'
    /// application setting must be configured to the Salesforce Organization IDs. The Organizational IDs can be found at
    /// <c>https://www.salesforce.com</c> under <c>Setup | Company Profile | Company Information</c>.
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
                var data = await ReadAsXmlAsync(request);
                var notifications = new SalesforceNotifications(data);

                // Ensure that the organization ID matches the expected value.
                var orgId = GetShortOrgId(notifications.OrganizationId);
                var secret = await GetReceiverConfig(request, ReceiverConfigName, id, 15, 18);
                var secretKey = GetShortOrgId(secret);
                if (!WebHookReceiver.SecretEqual(orgId, secretKey))
                {
                    var message = string.Format(CultureInfo.CurrentCulture, SalesforceReceiverResources.Receiver_BadValue, "OrganizationId");
                    context.Configuration.DependencyResolver.GetLogger().Error(message);
                    var fault = string.Format(CultureInfo.InvariantCulture, ReadResource("Microsoft.AspNet.WebHooks.Messages.FaultResponse.xml"), message);
                    var invalidId = GetXmlResponse(request, HttpStatusCode.BadRequest, fault);
                    return invalidId;
                }

                // Get the action
                var action = notifications.ActionId;
                if (string.IsNullOrEmpty(action))
                {
                    var message = string.Format(CultureInfo.CurrentCulture, SalesforceReceiverResources.Receiver_BadBody, "ActionId");
                    context.Configuration.DependencyResolver.GetLogger().Error(message);
                    var fault = string.Format(CultureInfo.InvariantCulture, ReadResource("Microsoft.AspNet.WebHooks.Messages.FaultResponse.xml"), message);
                    var badType = GetXmlResponse(request, HttpStatusCode.BadRequest, fault);
                    return badType;
                }

                // Call registered handlers
                var response = await ExecuteWebHookAsync(id, context, request, new string[] { action }, notifications);

                // Add SOAP response content if not already present or isn't XML. Ignore current (e.g. JSON) content.
                if (response?.Content == null || !response.Content.IsXml())
                {
                    // Ignore redirects because SOAP 1.1 doesn't mention them and they're corner cases in SOAP.
                    var statusCode = response?.StatusCode ?? HttpStatusCode.OK;
                    if (statusCode >= (HttpStatusCode)200 && statusCode < (HttpStatusCode)300)
                    {
                        var success = ReadResource("Microsoft.AspNet.WebHooks.Messages.NotificationResponse.xml");
                        response = GetXmlResponse(request, statusCode, success);
                    }
                    else
                    {
                        // Move failure information into a SOAP fault response. Fault contains code soapenv:Client and
                        // that must be transmitted with HTTP status 400, Bad Request according to SOAP 1.2 (mixing
                        // that sensible choice into this SOAP 1.1 implementation).
                        var resource = ReadResource("Microsoft.AspNet.WebHooks.Messages.FaultResponse.xml");
                        var faultString = string.Format(
                            CultureInfo.CurrentCulture,
                            SalesforceReceiverResources.Receiver_HandlerFailed,
                            statusCode,
                            response.ReasonPhrase);

                        var failure = string.Format(CultureInfo.InvariantCulture, resource, faultString);
                        response = GetXmlResponse(request, HttpStatusCode.BadRequest, failure);
                    }
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

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by caller.")]
        internal static HttpResponseMessage GetXmlResponse(HttpRequestMessage request, HttpStatusCode statusCode, string message)
        {
            var response = request.CreateResponse(statusCode);
            response.Content = new StringContent(message, Encoding.UTF8, "application/xml");
            return response;
        }

        internal static string ReadResource(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var content = assembly.GetManifestResourceStream(name);
            if (content == null)
            {
                var message = string.Format(CultureInfo.CurrentCulture, SalesforceReceiverResources.EmbeddedResources_Unknown, name);
                throw new InvalidOperationException(message);
            }

            using (var reader = new StreamReader(content))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
