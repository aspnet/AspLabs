// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.AspNetCore;
using System.Web.Http.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace System.Net.Http
{
    internal static class AspNetCoreHttpRequestMessageExtensions
    {
        private const string HttpContextKey = "MS_HttpContext";

        public static HttpContext GetHttpContext(this HttpRequestMessage request)
        {
            return (HttpContext)request.Properties[HttpContextKey];
        }

        public static HttpRequestMessage ToHttpRequestMessage(this HttpContext httpContext)
        {
            var httpRequest = httpContext.Request;
            var requestContent = CreateStreamedRequestContent(httpRequest);

            // Create the request
            var uriBuilder = new UriBuilder
            {
                Scheme = httpRequest.Scheme,
                Host = httpRequest.Host.Host,
                Port = httpRequest.Host.Port.GetValueOrDefault(80),
                Path = httpRequest.PathBase.Add(httpRequest.Path),
                Query = httpRequest.QueryString.Value
            };
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(httpRequest.Method), uriBuilder.Uri);

            try
            {
                // Set the body
                request.Content = requestContent;

                CopyHeaders(httpRequest, request);
            }
            catch
            {
                request.Dispose();
                throw;
            }

            // Set a request context on the request that lazily populates each property.
            HttpRequestContext requestContext = new AspNetCoreHttpRequestContext(httpContext, request);
            request.SetRequestContext(requestContext);

            request.Properties[HttpContextKey] = httpContext;

            return request;
        }

        public static void CopyHeaders(this HttpRequest httpRequest, HttpRequestMessage request)
        {
            // Copy the headers
            foreach (var (key, value) in httpRequest.Headers)
            {
                if (!request.Headers.TryAddWithoutValidation(key, (ICollection<string>)value))
                {
                    request.Content.Headers.TryAddWithoutValidation(key, (ICollection<string>)value);
                }
            }
        }

        public static void Copy(this HttpResponse httpResponse, HttpResponseMessage responseMessage)
        {
            // Copy the headers
            foreach (var (key, value) in httpResponse.Headers)
            {
                if (!responseMessage.Headers.TryAddWithoutValidation(key, (ICollection<string>)value))
                {
                    responseMessage.Content.Headers.TryAddWithoutValidation(key, (ICollection<string>)value);
                }
            }

            if (httpResponse.StatusCode != 0)
            {
                responseMessage.StatusCode = (HttpStatusCode)httpResponse.StatusCode;
            }
        }

        public static void ApplyTo(this HttpRequestMessage requestMessage, HttpContext httpContext)
        {
            foreach (var (key, value) in requestMessage.Headers.Concat(requestMessage.Content.Headers))
            {
                var currentHeaders = httpContext.Request.Headers[key];
                httpContext.Request.Headers[key] = new StringValues(currentHeaders.Concat(value).ToArray());
            }
        }

        private static HttpContent CreateStreamedRequestContent(HttpRequest httpRequest)
        {
            // Note that we must NOT dispose httpRequest.Body in this case. Disposing it would close the input
            // stream and prevent cascaded components from accessing it. The server MUST handle any necessary
            // cleanup upon request completion. NonOwnedStream prevents StreamContent (or its callers including
            // HttpRequestMessage) from calling Close or Dispose on httpRequest.Body.
            return new StreamContent(new NonDisposableStream(httpRequest.Body));
        }
    }
}
