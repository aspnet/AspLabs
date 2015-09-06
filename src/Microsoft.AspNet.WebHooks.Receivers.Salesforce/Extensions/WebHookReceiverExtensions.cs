// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml.Linq;
using Microsoft.AspNet.WebHooks.Properties;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Extension methods for <see cref="IWebHookReceiver"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class WebHookReceiverExtensions
    {
        /// <summary>
        /// Reads the XML HTTP request entity body.
        /// </summary>
        /// <remarks>This is a temporary fix until we release the next <see cref="WebHookReceiver"/> implementation.</remarks>
        /// <param name="receiver">The <see cref="IWebHookReceiver"/> to get the XML for.</param>
        /// <param name="request">The current <see cref="HttpRequestMessage"/>.</param>
        /// <returns>An <see cref="XElement"/> containing the HTTP request entity body.</returns>
        public static async Task<XElement> ReadBodyAsXmlAsync(this IWebHookReceiver receiver, HttpRequestMessage request)
        {
            if (receiver == null)
            {
                throw new ArgumentNullException("receiver");
            }

            // Check that there is a request body
            if (request.Content == null)
            {
                string msg = SalesforceReceiverResources.Receiver_NoBody;
                request.GetConfiguration().DependencyResolver.GetLogger().Info(msg);
                HttpResponseMessage noBody = request.CreateErrorResponse(HttpStatusCode.BadRequest, msg);
                throw new HttpResponseException(noBody);
            }

            // Check that the request body is XML
            if (!request.Content.IsXml())
            {
                string msg = SalesforceReceiverResources.Receiver_NoXml;
                request.GetConfiguration().DependencyResolver.GetLogger().Info(msg);
                HttpResponseMessage noXml = request.CreateErrorResponse(HttpStatusCode.UnsupportedMediaType, msg);
                throw new HttpResponseException(noXml);
            }

            try
            {
                // Read request body
                XElement result = await request.Content.ReadAsAsync<XElement>();
                return result;
            }
            catch (Exception ex)
            {
                string msg = SalesforceReceiverResources.Receiver_BadXml;
                request.GetConfiguration().DependencyResolver.GetLogger().Error(msg, ex);
                HttpResponseMessage invalidBody = request.CreateErrorResponse(HttpStatusCode.BadRequest, msg, ex);
                throw new HttpResponseException(invalidBody);
            }
        }
    }
}
