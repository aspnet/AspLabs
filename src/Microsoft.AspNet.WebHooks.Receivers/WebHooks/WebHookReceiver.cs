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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.WebHooks.Properties;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides an abstract <see cref="IWebHookReceiver"/> implementation which can be used to base other implementations on. 
    /// </summary>
    public abstract class WebHookReceiver : IWebHookReceiver
    {
        internal const string CodeQueryParameter = "code";

        /// <summary>
        /// Initializes a new instance of the <see cref="WebHookReceiver"/> class.
        /// </summary>
        protected WebHookReceiver()
        {
        }

        /// <inheritdoc />
        public abstract IEnumerable<string> Names { get; }

        /// <inheritdoc />
        public abstract Task<HttpResponseMessage> ReceiveAsync(string receiver, HttpRequestContext context, HttpRequestMessage request);

        /// <summary>
        /// Provides a time consistent comparison of two secrets in the form of two byte arrays.
        /// </summary>
        /// <param name="inputA">The first secret to compare.</param>
        /// <param name="inputB">The second secret to compare.</param>
        /// <returns>Returns <c>true</c> if the two secrets are equal, <c>false</c> otherwise.</returns>
        [MethodImpl(MethodImplOptions.NoOptimization)]
        protected internal static bool SecretEqual(byte[] inputA, byte[] inputB)
        {
            if (ReferenceEquals(inputA, inputB))
            {
                return true;
            }

            if (inputA == null || inputB == null || inputA.Length != inputB.Length)
            {
                return false;
            }

            bool areSame = true;
            for (int i = 0; i < inputA.Length; i++)
            {
                areSame &= inputA[i] == inputB[i];
            }
            return areSame;
        }

        /// <summary>
        /// Provides a time consistent comparison of two secrets in the form of two strings.
        /// </summary>
        /// <param name="inputA">The first secret to compare.</param>
        /// <param name="inputB">The second secret to compare.</param>
        /// <returns>Returns <c>true</c> if the two secrets are equal, <c>false</c> otherwise.</returns>
        [MethodImpl(MethodImplOptions.NoOptimization)]
        protected internal static bool SecretEqual(string inputA, string inputB)
        {
            if (ReferenceEquals(inputA, inputB))
            {
                return true;
            }

            if (inputA == null || inputB == null || inputA.Length != inputB.Length)
            {
                return false;
            }

            bool areSame = true;
            for (int i = 0; i < inputA.Length; i++)
            {
                areSame &= inputA[i] == inputB[i];
            }
            return areSame;
        }

        /// <summary>
        /// Some WebHooks rely on HTTPS for sending WebHook requests in a secure manner. A <see cref="WebHookReceiver"/>
        /// can call this method to ensure that the incoming WebHook request is using HTTPS. If the request is not
        /// using HTTPS an error will be generated and the request will not be further processed.
        /// </summary>
        /// <remarks>This method does allow local HTTP requests using <c>localhost</c>.</remarks>
        /// <param name="request">The current <see cref="HttpRequestMessage"/>.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by caller.")]
        protected virtual void EnsureSecureConnection(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (!request.IsLocal() && !request.RequestUri.IsHttps())
            {
                string msg = string.Format(CultureInfo.CurrentCulture, ReceiverResources.Receiver_NoHttps, GetType().Name, Uri.UriSchemeHttps);
                request.GetConfiguration().DependencyResolver.GetLogger().Error(msg);
                HttpResponseMessage noHttps = request.CreateErrorResponse(HttpStatusCode.BadRequest, msg);
                throw new HttpResponseException(noHttps);
            }
        }

        /// <summary>
        /// For WebHooks providers with insufficient security considerations, the receiver can require that the WebHook URI must 
        /// be an <c>https</c> URI and contain a 'code' query parameter with a value configured in the application setting with the name
        /// <paramref name="setting"/>.
        /// A sample WebHook URI is '<c>https://&lt;host&gt;/api/webhooks/incoming/&lt;receiver&gt;?code=c2e25cd9-6f63-44f6-bf2a-729bcc9f236a</c>'.
        /// The 'code' parameter must be between 32 and 64 characters long.
        /// </summary>
        /// <param name="request">The current <see cref="HttpRequestMessage"/>.</param>
        /// <param name="setting">The name of the application setting containing the expected 'code' query parameter value.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response is disposed by Web API.")]
        protected virtual void EnsureValidCode(HttpRequestMessage request, string setting)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            EnsureSecureConnection(request);

            NameValueCollection queryParameters = request.RequestUri.ParseQueryString();
            string code = queryParameters[CodeQueryParameter];
            if (string.IsNullOrEmpty(code))
            {
                string msg = string.Format(CultureInfo.CurrentCulture, ReceiverResources.Receiver_NoCode, CodeQueryParameter);
                request.GetConfiguration().DependencyResolver.GetLogger().Error(msg);
                HttpResponseMessage noCode = request.CreateErrorResponse(HttpStatusCode.BadRequest, msg);
                throw new HttpResponseException(noCode);
            }

            string secretKey = GetWebHookSecret(request, setting, 32, 64);
            if (!WebHookReceiver.SecretEqual(code, secretKey))
            {
                string msg = string.Format(CultureInfo.CurrentCulture, ReceiverResources.Receiver_BadCode, CodeQueryParameter);
                request.GetConfiguration().DependencyResolver.GetLogger().Error(msg);
                HttpResponseMessage invalidCode = request.CreateErrorResponse(HttpStatusCode.BadRequest, msg);
                throw new HttpResponseException(invalidCode);
            }
        }

        /// <summary>
        /// Gets the locally configured WebHook secret key used to validate any signature header provided in a WebHook request.
        /// </summary>
        /// <param name="request">The current <see cref="HttpRequestMessage"/>.</param>
        /// <param name="setting">The name of the application setting to look for.</param>
        /// <param name="minLength">The minimum length of the key value.</param>
        /// <param name="maxLength">The maximum length of the key value.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by caller")]
        protected virtual string GetWebHookSecret(HttpRequestMessage request, string setting, int minLength, int maxLength)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            HttpConfiguration config = request.GetConfiguration();
            string secret = config.DependencyResolver.GetSettings().GetValueOrDefault(setting);
            if (secret == null || secret.Length < minLength || secret.Length > maxLength)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, ReceiverResources.Receiver_BadSecret, setting, minLength, maxLength);
                config.DependencyResolver.GetLogger().Error(msg);
                HttpResponseMessage noSecret = request.CreateErrorResponse(HttpStatusCode.InternalServerError, msg);
                throw new HttpResponseException(noSecret);
            }
            return secret;
        }

        /// <summary>
        /// Gets the value of a given HTTP request header field. If the field is either not present or has more than one value
        /// then an error is generated.
        /// </summary>
        /// <param name="request">The current <see cref="HttpRequestMessage"/>.</param>
        /// <param name="requestHeaderName">The name of the HTTP request header to look up.</param>
        /// <returns>The signature header.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by caller")]
        protected virtual string GetRequestHeader(HttpRequestMessage request, string requestHeaderName)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            IEnumerable<string> headers;
            if (!request.Headers.TryGetValues(requestHeaderName, out headers) || headers.Count() != 1)
            {
                int headersCount = headers != null ? headers.Count() : 0;
                string msg = string.Format(CultureInfo.CurrentCulture, ReceiverResources.Receiver_BadHeader, requestHeaderName, headersCount);
                request.GetConfiguration().DependencyResolver.GetLogger().Info(msg);
                HttpResponseMessage noHeader = request.CreateErrorResponse(HttpStatusCode.BadRequest, msg);
                throw new HttpResponseException(noHeader);
            }

            return headers.First();
        }

        /// <summary>
        /// Reads the JSON HTTP request entity body.
        /// </summary>
        /// <param name="request">The current <see cref="HttpRequestMessage"/>.</param>
        /// <returns>A <see cref="JObject"/> containing the HTTP request entity body.</returns>
        protected virtual async Task<JObject> ReadAsJsonAsync(HttpRequestMessage request)
        {
            // Check that there is a request body
            if (request.Content == null)
            {
                string msg = ReceiverResources.Receiver_NoBody;
                request.GetConfiguration().DependencyResolver.GetLogger().Info(msg);
                HttpResponseMessage noBody = request.CreateErrorResponse(HttpStatusCode.BadRequest, msg);
                throw new HttpResponseException(noBody);
            }

            // Check that the request body is JSON
            if (!request.Content.IsJson())
            {
                string msg = ReceiverResources.Receiver_NoJson;
                request.GetConfiguration().DependencyResolver.GetLogger().Info(msg);
                HttpResponseMessage noJson = request.CreateErrorResponse(HttpStatusCode.UnsupportedMediaType, msg);
                throw new HttpResponseException(noJson);
            }

            try
            {
                // Read request body
                JObject result = await request.Content.ReadAsAsync<JObject>();
                return result;
            }
            catch (Exception ex)
            {
                string msg = ReceiverResources.Receiver_BadJson;
                request.GetConfiguration().DependencyResolver.GetLogger().Error(msg, ex);
                HttpResponseMessage invalidBody = request.CreateErrorResponse(HttpStatusCode.BadRequest, msg, ex);
                throw new HttpResponseException(invalidBody);
            }
        }

        /// <summary>
        /// Reads the HTML Form Data HTTP request entity body.
        /// </summary>
        /// <param name="request">The current <see cref="HttpRequestMessage"/>.</param>
        /// <returns>A <see cref="NameValueCollection"/> containing the HTTP request entity body.</returns>
        protected virtual async Task<NameValueCollection> ReadAsFormDataAsync(HttpRequestMessage request)
        {
            // Check that there is a request body
            if (request.Content == null)
            {
                string msg = ReceiverResources.Receiver_NoBody;
                request.GetConfiguration().DependencyResolver.GetLogger().Info(msg);
                HttpResponseMessage noBody = request.CreateErrorResponse(HttpStatusCode.BadRequest, msg);
                throw new HttpResponseException(noBody);
            }

            // Check that the request body is form data
            if (!request.Content.IsFormData())
            {
                string msg = ReceiverResources.Receiver_NoFormData;
                request.GetConfiguration().DependencyResolver.GetLogger().Info(msg);
                HttpResponseMessage noJson = request.CreateErrorResponse(HttpStatusCode.UnsupportedMediaType, msg);
                throw new HttpResponseException(noJson);
            }

            try
            {
                // Read request body
                NameValueCollection result = await request.Content.ReadAsFormDataAsync();
                return result;
            }
            catch (Exception ex)
            {
                string msg = ReceiverResources.Receiver_BadFormData;
                request.GetConfiguration().DependencyResolver.GetLogger().Error(msg, ex);
                HttpResponseMessage invalidBody = request.CreateErrorResponse(HttpStatusCode.BadRequest, msg, ex);
                throw new HttpResponseException(invalidBody);
            }
        }

        /// <summary>
        ///  Creates a 405 "Method Not Allowed" response which a receiver can use to indicate that a request with a 
        ///  non-support HTTP method could not be processed.
        /// </summary>
        /// <param name="request">The current <see cref="HttpRequestMessage"/>.</param>
        /// <returns>A fully initialized "Method Not Allowed" <see cref="HttpResponseMessage"/>.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by caller")]
        protected virtual HttpResponseMessage CreateBadMethodResponse(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            string msg = string.Format(CultureInfo.CurrentCulture, ReceiverResources.Receiver_BadMethod, request.Method, GetType().Name);
            request.GetConfiguration().DependencyResolver.GetLogger().Error(msg);
            HttpResponseMessage badMethod = request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, msg);
            return badMethod;
        }

        /// <summary>
        ///  Creates a 400 "Bad Request" response which a receiver can use to indicate that a request had an invalid signature 
        ///  and as a result could not be processed.
        /// </summary>
        /// <param name="request">The current <see cref="HttpRequestMessage"/>.</param>
        /// <param name="signatureHeaderName">The name of the HTTP header with invalid contents.</param>
        /// <returns>A fully initialized "Bad Request" <see cref="HttpResponseMessage"/>.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by caller")]
        protected virtual HttpResponseMessage CreateBadSignatureResponse(HttpRequestMessage request, string signatureHeaderName)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            string msg = string.Format(CultureInfo.CurrentCulture, ReceiverResources.Receiver_BadSignature, signatureHeaderName, GetType().Name);
            request.GetConfiguration().DependencyResolver.GetLogger().Error(msg);
            HttpResponseMessage badSignature = request.CreateErrorResponse(HttpStatusCode.BadRequest, msg);
            return badSignature;
        }

        /// <summary>
        /// Processes the WebHook request by calling all registered <see cref="IWebHookHandler"/> instances. 
        /// </summary>
        /// <param name="receiver">The case-insensitive name of the receiver used by the incoming WebHook. The receiver 
        /// name can for example be <c>dropbox</c> or <c>github</c>.</param>
        /// <param name="context">The <see cref="HttpRequestContext"/> for this WebHook invocation.</param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> for this WebHook invocation.</param>
        /// <param name="actions">The collection of actions associated with this WebHook invocation.</param>
        /// <param name="data">Optional data associated with this WebHook invocation.</param>
        protected virtual async Task<HttpResponseMessage> ExecuteWebHookAsync(string receiver, HttpRequestContext context, HttpRequestMessage request, IEnumerable<string> actions, object data)
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
            if (actions == null)
            {
                actions = new string[0];
            }

            // Execute handlers. Note that we wait for them to complete before
            // we return. This means that we don't send back the final response
            // before all handlers have executed. As a result we expect handlers
            // to be fairly quick in what they process. If a handler sets the 
            // Response property on the context then the execution is stopped 
            // and that response returned. If a handler throws an exception then
            // the execution of handlers is also stopped.
            WebHookHandlerContext handlerContext = new WebHookHandlerContext(actions)
            {
                Data = data,
                Request = request,
                RequestContext = context,
            };

            receiver = receiver.ToLowerInvariant();
            IEnumerable<IWebHookHandler> handlers = context.Configuration.DependencyResolver.GetHandlers();
            foreach (IWebHookHandler handler in handlers)
            {
                // Only call handlers with matching receiver name (or no receiver name in which case they support all receivers)
                if (handler.Receiver != null && !string.Equals(receiver, handler.Receiver, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                await handler.ExecuteAsync(receiver, handlerContext);

                // Check if response has been set and if so stop the processing.
                if (handlerContext.Response != null)
                {
                    return handlerContext.Response;
                }
            }
            return request.CreateResponse();
        }
    }
}
