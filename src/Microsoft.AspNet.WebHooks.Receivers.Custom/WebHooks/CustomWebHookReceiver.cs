// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.AspNet.WebHooks.Properties;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides an <see cref="IWebHookReceiver"/> implementation that can be used to receive WebHooks from multiple parties supporting WebHooks generated 
    /// by the ASP.NET Custom WebHooks module. Each party can have a separate shared secret used to sign its WebHook requests. Define an application
    /// setting named '<c>MS_WebHookReceiverSecret_Custom_&lt;name&gt;</c>' containing the secret for each WebHook generator. The 
    /// corresponding WebHook URI is of the form '<c>https://&lt;host&gt;/api/webhooks/incoming/&lt;name&gt;</c>'.
    /// </summary>
    public class CustomWebHookReceiver : WebHookReceiver
    {
        internal const string ReceiverName = "custom";
        internal const string SecretKeyPrefix = "MS_WebHookReceiverSecret_Custom_";

        internal const string EchoParameter = "echo";
        internal const string SignatureHeaderKey = "sha256";
        internal const string SignatureHeaderValueTemplate = SignatureHeaderKey + "={0}";
        internal const string SignatureHeaderName = "ms-signature";
        internal const string BodyActionsKey = "Actions";

        private readonly IEnumerable<string> _names;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomWebHookReceiver"/> class.
        /// </summary>
        public CustomWebHookReceiver()
            : this(WebHooksConfig.Config)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomWebHookReceiver"/> class with the given <paramref name="config"/>.
        /// </summary>
        /// <param name="config">The <see cref="HttpConfiguration"/> to use for resolving dependencies.</param>
        public CustomWebHookReceiver(HttpConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            _names = GetNames(config);
        }

        /// <inheritdoc />
        public override IEnumerable<string> Names
        {
            get { return _names; }
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
                await VerifySignature(receiver, request);

                // Read the request entity body
                JObject data = await ReadAsJsonAsync(request);

                // Pick out actions from data
                var rawActions = data[BodyActionsKey];
                IEnumerable<string> actions = rawActions != null ? rawActions.Values<string>() : Enumerable.Empty<string>();

                // Call registered handlers
                return await ExecuteWebHookAsync(receiver, context, request, actions, data);
            }
            else if (request.Method == HttpMethod.Get)
            {
                return WebHookVerification(receiver, request);
            }
            else
            {
                return CreateBadMethodResponse(request);
            }
        }

        internal static IEnumerable<string> GetNames(HttpConfiguration config)
        {
            SettingsDictionary settings = config.DependencyResolver.GetSettings();
            string[] matches = settings.Keys.Where(key => key.StartsWith(SecretKeyPrefix, StringComparison.OrdinalIgnoreCase) && key.Length > SecretKeyPrefix.Length)
                .Select(m => m.Substring(SecretKeyPrefix.Length).ToLowerInvariant())
                .ToArray();

            if (matches.Length > 0)
            {
                string registeredNames = string.Join(", ", matches);
                string msg = string.Format(CultureInfo.CurrentCulture, CustomReceiverResources.Receiver_Names, typeof(CustomWebHookReceiver).Name, registeredNames);
                config.DependencyResolver.GetLogger().Info(msg);
            }
            else
            {
                string keyFormat = SecretKeyPrefix + "<name>";
                string msg = string.Format(CultureInfo.CurrentCulture, CustomReceiverResources.Receiver_NoNames, keyFormat, typeof(CustomWebHookReceiver).Name);
                config.DependencyResolver.GetLogger().Error(msg);
            }

            return matches;
        }

        /// <summary>
        /// Verifies that the signature header matches that of the actual body.
        /// </summary>
        protected virtual async Task VerifySignature(string receiver, HttpRequestMessage request)
        {
            string secretKey = GetCustomWebHookSecret(receiver, request);

            // Get the expected hash from the signature header
            string header = GetRequestHeader(request, SignatureHeaderName);
            string[] values = header.SplitAndTrim('=');
            if (values.Length != 2 || !string.Equals(values[0], SignatureHeaderKey, StringComparison.OrdinalIgnoreCase))
            {
                string msg = string.Format(CultureInfo.CurrentCulture, CustomReceiverResources.Receiver_BadHeaderValue, SignatureHeaderName, SignatureHeaderKey, "<value>");
                request.GetConfiguration().DependencyResolver.GetLogger().Error(msg);
                HttpResponseMessage invalidHeader = request.CreateErrorResponse(HttpStatusCode.BadRequest, msg);
                throw new HttpResponseException(invalidHeader);
            }

            byte[] expectedHash;
            try
            {
                expectedHash = EncodingUtilities.FromHex(values[1]);
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, CustomReceiverResources.Receiver_BadHeaderEncoding, SignatureHeaderName);
                request.GetConfiguration().DependencyResolver.GetLogger().Error(msg, ex);
                HttpResponseMessage invalidEncoding = request.CreateErrorResponse(HttpStatusCode.BadRequest, msg);
                throw new HttpResponseException(invalidEncoding);
            }

            // Compute the actual hash of the request body
            byte[] actualHash;
            byte[] secret = Encoding.UTF8.GetBytes(secretKey);
            using (var hasher = new HMACSHA256(secret))
            {
                byte[] data = await request.Content.ReadAsByteArrayAsync();
                actualHash = hasher.ComputeHash(data);
            }

            // Now verify that the actual hash matches the expected hash.
            if (!WebHookReceiver.SecretEqual(expectedHash, actualHash))
            {
                HttpResponseMessage badSignature = CreateBadSignatureResponse(request, SignatureHeaderName);
                throw new HttpResponseException(badSignature);
            }
        }

        /// <summary>
        /// Creates a response to a WebHook verification GET request.
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by caller")]
        protected virtual HttpResponseMessage WebHookVerification(string receiver, HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            // Verify that we have the secret as an app setting
            GetCustomWebHookSecret(receiver, request);

            // Get the 'echo' parameter and echo it back to caller
            NameValueCollection queryParameters = request.RequestUri.ParseQueryString();
            string echo = queryParameters[EchoParameter];
            if (string.IsNullOrEmpty(echo))
            {
                string msg = string.Format(CultureInfo.CurrentCulture, CustomReceiverResources.Receiver_NoEcho, EchoParameter);
                request.GetConfiguration().DependencyResolver.GetLogger().Error(msg);
                HttpResponseMessage noEcho = request.CreateErrorResponse(HttpStatusCode.BadRequest, msg);
                return noEcho;
            }

            // Return the echo response
            HttpResponseMessage echoResponse = request.CreateResponse();
            echoResponse.Content = new StringContent(echo);
            return echoResponse;
        }

        /// <summary>
        /// Gets the application settings key containing the shared secret for a given <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The receiver for which to look up the shared key.</param>
        /// <param name="request">The current <see cref="HttpRequestMessage"/>.</param>
        /// <returns>The resulting shared secret.</returns>
        protected virtual string GetCustomWebHookSecret(string receiver, HttpRequestMessage request)
        {
            if (receiver == null)
            {
                throw new ArgumentNullException("receiver");
            }
            if (request == null)
            {
                throw new ArgumentNullException("receiver");
            }

            string secretKey = SecretKeyPrefix + receiver;
            string secret = GetWebHookSecret(request, secretKey, 32, 64);
            return secret;
        }
    }
}
