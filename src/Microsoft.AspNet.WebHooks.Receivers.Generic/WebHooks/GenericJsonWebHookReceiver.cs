// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides an <see cref="IWebHookReceiver"/> implementation which supports generic WebHooks 
    /// with no special validation logic or security requirements. This can for example be used 
    /// to receive WebHooks from IFTTT's Maker Channel or a Zapier WebHooks Action.
    /// A sample WebHook URI is '<c>https://&lt;host&gt;/api/webhooks/incoming/genericjson/{id}?code=83699ec7c1d794c0c780e49a5c72972590571fd8</c>'.
    /// For security reasons the WebHook URI must be an <c>https</c> URI and contain a 'code' query parameter with the
    /// same value as configured in the '<c>MS_WebHookReceiverSecret_GenericJson</c>' application setting, optionally using IDs
    /// to differentiate between multiple WebHooks, for example '<c>secret0, id1=secret1, id2=secret2</c>'.
    /// The 'code' parameter must be between 32 and 128 characters long.
    /// The URI may optionally include a '<c>action</c>' query parameter which will serve as the WebHook action.
    /// </summary>
    public class GenericJsonWebHookReceiver : WebHookReceiver
    {
        internal const string ReceiverName = "genericjson";
        internal const string ActionQueryParameter = "action";
        internal const string DefaultAction = "change";

        /// <inheritdoc />
        public override string Name
        {
            get { return ReceiverName; }
        }

        /// <inheritdoc />
        public override async Task<HttpResponseMessage> ReceiveAsync(string id, HttpRequestContext context, HttpRequestMessage request)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (request.Method != HttpMethod.Post)
            {
                return CreateBadMethodResponse(request);
            }

            // Ensure that we use https and have a valid code parameter
            await EnsureValidCode(request, id);

            // Read the request entity body
            JObject data = await ReadAsJsonAsync(request);

            // Get the action
            NameValueCollection queryParameters = request.RequestUri.ParseQueryString();
            string action = queryParameters[ActionQueryParameter];
            if (string.IsNullOrEmpty(action))
            {
                action = DefaultAction;
            }

            // Call registered handlers
            return await ExecuteWebHookAsync(id, context, request, new[] { action }, data);
        }
    }
}
