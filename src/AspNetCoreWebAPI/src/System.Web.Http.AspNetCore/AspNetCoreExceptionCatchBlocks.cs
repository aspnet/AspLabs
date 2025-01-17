// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Hosting;

namespace System.Web.Http.AspNetCore
{
    /// <summary>Provides the catch blocks used within this assembly.</summary>
    public static class AspNetCoreExceptionCatchBlocks
    {
        private static readonly ExceptionContextCatchBlock _httpMessageHandlerAdapterBufferContent =
            new ExceptionContextCatchBlock(typeof(HttpMessageHandlerAdapter).Name + ".BufferContent",
                isTopLevel: true, callsHandler: true);
        private static readonly ExceptionContextCatchBlock _httpMessageHandlerAdapterBufferError =
            new ExceptionContextCatchBlock(typeof(HttpMessageHandlerAdapter).Name + ".BufferError", isTopLevel: true,
                callsHandler: false);
        private static readonly ExceptionContextCatchBlock _httpMessageHandlerAdapterComputeContentLength =
            new ExceptionContextCatchBlock(typeof(HttpMessageHandlerAdapter).Name + ".ComputeContentLength",
                isTopLevel: true, callsHandler: false);
        private static readonly ExceptionContextCatchBlock _httpMessageHandlerAdapterStreamContent =
            new ExceptionContextCatchBlock(typeof(HttpMessageHandlerAdapter).Name + ".StreamContent",
                isTopLevel: true, callsHandler: false);

        /// <summary>Gets the catch block in <see cref="HttpMessageHandlerAdapter"/>.BufferContent.</summary>
        /// <remarks>
        /// This catch block handles exceptions when writing the <see cref="HttpContent"/> under an
        /// <see cref="IHostBufferPolicySelector"/> that buffers.
        /// </remarks>
        public static ExceptionContextCatchBlock HttpMessageHandlerAdapterBufferContent
        {
            get
            {
                return _httpMessageHandlerAdapterBufferContent;
            }
        }

        /// <summary>Gets the catch block in <see cref="HttpMessageHandlerAdapter"/>.BufferError.</summary>
        /// <remarks>
        /// This catch block handles exceptions when writing the <see cref="HttpContent"/> of the error response itself
        /// (after <see cref="HttpMessageHandlerAdapterBufferContent"/>).
        /// </remarks>
        public static ExceptionContextCatchBlock HttpMessageHandlerAdapterBufferError
        {
            get
            {
                return _httpMessageHandlerAdapterBufferError;
            }
        }

        /// <summary>Gets the catch block in <see cref="HttpMessageHandlerAdapter"/>.ComputeContentLength.</summary>
        /// <remarks>
        /// This catch block handles exceptions when calling <see cref="HttpContent.TryComputeLength"/>.
        /// </remarks>
        public static ExceptionContextCatchBlock HttpMessageHandlerAdapterComputeContentLength
        {
            get
            {
                return _httpMessageHandlerAdapterComputeContentLength;
            }
        }

        /// <summary>Gets the catch block in <see cref="HttpMessageHandlerAdapter"/>.StreamContent.</summary>
        /// <remarks>
        /// This catch block handles exceptions when writing the <see cref="HttpContent"/> under an
        /// <see cref="IHostBufferPolicySelector"/> that does not buffer.
        /// </remarks>
        public static ExceptionContextCatchBlock HttpMessageHandlerAdapterStreamContent
        {
            get
            {
                return _httpMessageHandlerAdapterStreamContent;
            }
        }
    }
}
