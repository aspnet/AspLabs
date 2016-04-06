// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.TestUtilities.Mocks
{
    /// <summary>
    /// Provides an <see cref="HttpMessageHandler"/> where the response can be modified in flight by providing
    /// an appropriate <see cref="Handler"/>.
    /// </summary>
    public class HttpMessageHandlerMock : HttpMessageHandler
    {
        /// <summary>
        /// Gets or sets the request counter.
        /// </summary>
        public int Counter { get; set; }

        /// <summary>
        /// The handler which will process the <see cref="HttpRequestMessage"/> and return an <see cref="HttpResponseMessage"/>.
        /// </summary>
        public Func<HttpRequestMessage, int, Task<HttpResponseMessage>> Handler { get; set; }

        /// <inheritdoc />
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Handler(request, Counter++);
        }
    }
}
