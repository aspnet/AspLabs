// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.Json;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Shared.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Internal.CallHandlers
{
    internal sealed class ServerStreamingServerCallHandler<TService, TRequest, TResponse> : ServerCallHandlerBase<TService, TRequest, TResponse>
        where TService : class
        where TRequest : class
        where TResponse : class
    {
        private readonly ServerStreamingServerMethodInvoker<TService, TRequest, TResponse> _invoker;

        public ServerStreamingServerCallHandler(
            ServerStreamingServerMethodInvoker<TService, TRequest, TResponse> unaryMethodInvoker,
            ILoggerFactory loggerFactory,
            CallHandlerDescriptorInfo descriptorInfo,
            JsonSerializerOptions options) : base(unaryMethodInvoker, loggerFactory, descriptorInfo, options)
        {
            _invoker = unaryMethodInvoker;
        }

        protected override async Task HandleCallAsyncCore(HttpContext httpContext, HttpApiServerCallContext serverCallContext)
        {
            // Decode request
            var request = await JsonRequestHelpers.ReadMessage<TRequest>(serverCallContext, SerializerOptions);

            var streamWriter = new HttpContextStreamWriter<TResponse>(serverCallContext, SerializerOptions);
            try
            {
                await _invoker.Invoke(httpContext, serverCallContext, request, streamWriter);
            }
            finally
            {
                streamWriter.Complete();
            }
        }

        protected override bool IsStreaming => true;
    }
}
