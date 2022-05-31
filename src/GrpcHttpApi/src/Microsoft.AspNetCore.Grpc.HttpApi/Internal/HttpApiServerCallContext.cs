// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Grpc.AspNetCore.Server;
using Grpc.Core;
using Grpc.Gateway.Runtime;
using Grpc.Shared.Server;
using Microsoft.AspNetCore.Grpc.HttpApi.Internal;
using Microsoft.AspNetCore.Grpc.HttpApi.Internal.CallHandlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Internal
{
    internal class HttpApiServerCallContext : ServerCallContext, IServerCallContextFeature
    {
        private readonly IMethod _method;

        public HttpContext HttpContext { get; }
        public MethodOptions Options { get; }
        public CallHandlerDescriptorInfo DescriptorInfo { get; }
        public bool IsJsonRequestContent { get; }
        public Encoding RequestEncoding { get; }

        internal ILogger Logger { get; }

        private string? _peer;
        private Metadata? _requestHeaders;

        public HttpApiServerCallContext(HttpContext httpContext, MethodOptions options, IMethod method, CallHandlerDescriptorInfo descriptorInfo, ILogger logger)
        {
            HttpContext = httpContext;
            Options = options;
            _method = method;
            DescriptorInfo = descriptorInfo;
            Logger = logger;
            IsJsonRequestContent = JsonRequestHelpers.HasJsonContentType(httpContext.Request, out var charset);
            RequestEncoding = JsonRequestHelpers.GetEncodingFromCharset(charset) ?? Encoding.UTF8;

            // Add the HttpContext to UserState so GetHttpContext() continues to work
            HttpContext.Items["__HttpContext"] = httpContext;
        }

        public ServerCallContext ServerCallContext => this;

        protected override string MethodCore => _method.FullName;

        protected override string HostCore => HttpContext.Request.Host.Value;

        protected override string? PeerCore
        {
            get
            {
                // Follows the standard at https://github.com/grpc/grpc/blob/master/doc/naming.md
                if (_peer == null)
                {
                    var connection = HttpContext.Connection;
                    if (connection.RemoteIpAddress != null)
                    {
                        switch (connection.RemoteIpAddress.AddressFamily)
                        {
                            case AddressFamily.InterNetwork:
                                _peer = "ipv4:" + connection.RemoteIpAddress + ":" + connection.RemotePort;
                                break;
                            case AddressFamily.InterNetworkV6:
                                _peer = "ipv6:[" + connection.RemoteIpAddress + "]:" + connection.RemotePort;
                                break;
                            default:
                                // TODO(JamesNK) - Test what should be output when used with UDS and named pipes
                                _peer = "unknown:" + connection.RemoteIpAddress + ":" + connection.RemotePort;
                                break;
                        }
                    }
                }

                return _peer;
            }
        }

        internal async Task ProcessHandlerErrorAsync(Exception ex, string method, bool isStreaming, JsonSerializerOptions options)
        {
            Status status;
            if (ex is RpcException rpcException)
            {
                // RpcException is thrown by client code to modify the status returned from the server.
                // Log the status and detail. Don't log the exception to reduce log verbosity.
                GrpcServerLog.RpcConnectionError(Logger, rpcException.StatusCode, rpcException.Status.Detail);

                status = rpcException.Status;
            }
            else
            {
                GrpcServerLog.ErrorExecutingServiceMethod(Logger, method, ex);

                var message = ErrorMessageHelper.BuildErrorMessage("Exception was thrown by handler.", ex, Options.EnableDetailedErrors);

                // Note that the exception given to status won't be returned to the client.
                // It is still useful to set in case an interceptor accesses the status on the server.
                status = new Status(StatusCode.Unknown, message, ex);
            }

            await JsonRequestHelpers.SendErrorResponse(HttpContext.Response, RequestEncoding, status, options);
            if (isStreaming)
            {
                await HttpContext.Response.Body.WriteAsync(GrpcProtocolConstants.StreamingDelimiter);
            }
        }

        internal Task EndCallAsync()
        {
            return Task.CompletedTask;
        }

        protected override DateTime DeadlineCore { get; }

        protected override Metadata RequestHeadersCore
        {
            get
            {
                if (_requestHeaders == null)
                {
                    _requestHeaders = new Metadata();

                    foreach (var header in HttpContext.Request.Headers)
                    {
                        // gRPC metadata contains a subset of the request headers
                        // Filter out pseudo headers (start with :) and other known headers
                        if (header.Key.StartsWith(':') || GrpcProtocolConstants.FilteredHeaders.Contains(header.Key))
                        {
                            continue;
                        }
                        else if (header.Key.EndsWith(Metadata.BinaryHeaderSuffix, StringComparison.OrdinalIgnoreCase))
                        {
                            _requestHeaders.Add(header.Key, GrpcProtocolHelpers.ParseBinaryHeader(header.Value));
                        }
                        else
                        {
                            _requestHeaders.Add(header.Key, header.Value);
                        }
                    }
                }

                return _requestHeaders;
            }
        }

        protected override CancellationToken CancellationTokenCore => HttpContext.RequestAborted;

        protected override Metadata ResponseTrailersCore => throw new NotImplementedException();

        protected override Status StatusCore { get; set; }

        protected override WriteOptions WriteOptionsCore
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        protected override AuthContext AuthContextCore => throw new NotImplementedException();

        protected override IDictionary<object, object?> UserStateCore => HttpContext.Items;

        protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions options)
        {
            throw new NotImplementedException();
        }

        protected override async Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
        {
            // Headers can only be written once. Throw on subsequent call to write response header instead of silent no-op.
            if (HttpContext.Response.HasStarted)
            {
                throw new InvalidOperationException("Response headers can only be sent once per call.");
            }

            if (responseHeaders != null)
            {
                foreach (var entry in responseHeaders)
                {
                    if (entry.IsBinary)
                    {
                        HttpContext.Response.Headers[entry.Key] = Convert.ToBase64String(entry.ValueBytes);
                    }
                    else
                    {
                        HttpContext.Response.Headers[entry.Key] = entry.Value;
                    }
                }
            }

            await HttpContext.Response.BodyWriter.FlushAsync();
        }
    }
}
