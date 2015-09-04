// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Proxy
{
    public class ProxyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly HttpClient _httpClient;
        private readonly ProxyOptions _options;

        public ProxyMiddleware([NotNull] RequestDelegate next, [NotNull] ProxyOptions options)
        {
            _next = next;
            if (string.IsNullOrEmpty(options.Host))
            {
                throw new ArgumentException("Options parameter must specify host.", "options");
            }

            // Setting default Port and Scheme if not specified
            if (string.IsNullOrEmpty(options.Port))
            {
                if (string.Equals(options.Scheme, "https", StringComparison.OrdinalIgnoreCase))
                {
                    options.Port = "443";
                }
                else
                {
                    options.Port = "80";
                }

            }

            if (string.IsNullOrEmpty(options.Scheme))
            {
                options.Scheme = "http";
            }

            _options = options;

#if DNX451
            _httpClient = new HttpClient(_options.BackChannelMessageHandler?? new HttpClientHandler());
#else
            _httpClient = new HttpClient(_options.BackChannelMessageHandler?? new Net.Http.Client.ManagedHandler());
#endif

        }

        public async Task Invoke(HttpContext context)
        {
            var requestMessage = new HttpRequestMessage();
            if (!string.Equals(context.Request.Method, "GET", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(context.Request.Method, "HEAD", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(context.Request.Method, "DELETE", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(context.Request.Method, "TRACE", StringComparison.OrdinalIgnoreCase))
            {
                var streamContent = new StreamContent(context.Request.Body);
                requestMessage.Content = streamContent;
            }

            // Copy the request headers
            foreach (var header in context.Request.Headers)
            {
                if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()) && requestMessage.Content != null)
                {
                    requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }

            requestMessage.Headers.Host = _options.Host + ":" + _options.Port;
            var uriString = $"{_options.Scheme}://{_options.Host}:{_options.Port}{context.Request.PathBase}{context.Request.Path}{context.Request.QueryString}";
            requestMessage.RequestUri = new Uri(uriString);
            requestMessage.Method = new HttpMethod(context.Request.Method);
            using (var responseMessage = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted))
            {
                context.Response.StatusCode = (int)responseMessage.StatusCode;
                foreach (var header in responseMessage.Headers)
                {
                    context.Response.Headers[header.Key] = header.Value.ToArray();
                }

                foreach (var header in responseMessage.Content.Headers)
                {
                    context.Response.Headers[header.Key] = header.Value.ToArray();
                }

                // SendAsync removes chunking from the response. This removes the header so it doesn't expect a chunked response.
                context.Response.Headers.Remove("transfer-encoding");
                await responseMessage.Content.CopyToAsync(context.Response.Body);
            }
        }
    }
}
