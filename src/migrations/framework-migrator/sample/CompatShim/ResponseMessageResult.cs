// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace System.Web.Http
{
    /// <summary>
    /// An action result that returns a specified response message.
    /// </summary>
    public class ResponseMessageResult : IHttpActionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseMessageResult"/> class.
        /// </summary>
        /// <param name="response">The response message.</param>
        public ResponseMessageResult(HttpResponseMessage response)
        {
            ResponseMessage = response ?? throw new ArgumentNullException(nameof(response));
        }

        /// <summary>
        /// Gets the response message.
        /// </summary>
        public HttpResponseMessage ResponseMessage { get; }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;
            using (ResponseMessage)
            {
                response.StatusCode = (int)ResponseMessage.StatusCode;

                var responseFeature = context.HttpContext.Features.Get<IHttpResponseFeature>();
                if (responseFeature != null)
                {
                    responseFeature.ReasonPhrase = ResponseMessage.ReasonPhrase;
                }

                var responseHeaders = ResponseMessage.Headers;

                // Ignore the Transfer-Encoding header if it is just "chunked".
                // We let the host decide about whether the response should be chunked or not.
                if (responseHeaders.TransferEncodingChunked == true &&
                    responseHeaders.TransferEncoding.Count == 1)
                {
                    responseHeaders.TransferEncoding.Clear();
                }

                foreach (var header in responseHeaders)
                {
                    response.Headers.Append(header.Key, header.Value.ToArray());
                }

                if (ResponseMessage.Content != null)
                {
                    var contentHeaders = ResponseMessage.Content.Headers;

                    // Copy the response content headers only after ensuring they are complete.
                    // We ask for Content-Length first because HttpContent lazily computes this
                    // and only afterwards writes the value into the content headers.
                    var unused = contentHeaders.ContentLength;

                    foreach (var header in contentHeaders)
                    {
                        response.Headers.Append(header.Key, header.Value.ToArray());
                    }

                    await ResponseMessage.Content.CopyToAsync(response.Body);
                }
            }
        }
    }
}
