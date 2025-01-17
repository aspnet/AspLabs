// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http.AspNetCore
{
    internal static class HttpMessageHandlerExtensions
    {
        public static Task<HttpResponseMessage> SendAsync(this HttpMessageHandler handler, HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HttpMessageInvoker invoker = new HttpMessageInvoker(handler, disposeHandler: false);
            return invoker.SendAsync(request, cancellationToken);
        }
    }
}
