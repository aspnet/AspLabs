// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Hosting;

namespace System.Web.Http.AspNetCore
{
    /// <summary>
    /// Provides the default implementation of <see cref="IHostBufferPolicySelector"/> used by the ASP.NET Core Web API adapter.
    /// </summary>
    public class AspNetCoreBufferPolicySelector : IHostBufferPolicySelector
    {
        private readonly bool _bufferRequests;

        public AspNetCoreBufferPolicySelector(bool bufferRequests) => _bufferRequests = bufferRequests;

        /// <inheritdoc />
        public bool UseBufferedInputStream(object hostContext)
        {
            return _bufferRequests;
        }

        /// <inheritdoc />
        public bool UseBufferedOutputStream(HttpResponseMessage response)
        {
            HttpContent content = response.Content;
            if (content == null)
            {
                return false;
            }

            // Any HttpContent that knows its length is presumably already buffered internally.
            long? contentLength = content.Headers.ContentLength;
            if (contentLength.HasValue && contentLength.Value >= 0)
            {
                return false;
            }

            // If the response is meant to use chunked transfer encoding, don't buffer.
            bool? transferEncodingChunked = response.Headers.TransferEncodingChunked;
            if (transferEncodingChunked.HasValue && transferEncodingChunked.Value)
            {
                return false;
            }

            // Content length is null or -1 (meaning not known).
            // Buffer any HttpContent except StreamContent and PushStreamContent
            if (content is StreamContent || content is PushStreamContent)
            {
                return false;
            }

            return true;
        }
    }
}
