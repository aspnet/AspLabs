// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Grpc.HttpApi
{
    internal class HttpApiServerCallContext : ServerCallContext
    {
        private readonly HttpContext _httpContext;
        private readonly string _methodFullName;
        private string? _peer;

        public HttpApiServerCallContext(HttpContext httpContext, string methodFullName)
        {
            _httpContext = httpContext;
            _methodFullName = methodFullName;

            // Add the HttpContext to UserState so GetHttpContext() continues to work
            _httpContext.Items["__HttpContext"] = httpContext;
        }

        protected override string MethodCore => _methodFullName;
        protected override string HostCore => _httpContext.Request.Host.Value;

        protected override string? PeerCore
        {
            get
            {
                // Follows the standard at https://github.com/grpc/grpc/blob/master/doc/naming.md
                if (_peer == null)
                {
                    var connection = _httpContext.Connection;
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

        protected override DateTime DeadlineCore { get; }
        protected override Metadata RequestHeadersCore => throw new NotImplementedException();
        protected override CancellationToken CancellationTokenCore => _httpContext.RequestAborted;
        protected override Metadata ResponseTrailersCore => throw new NotImplementedException();
        protected override Status StatusCore { get; set; }
        protected override WriteOptions WriteOptionsCore
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
        protected override AuthContext AuthContextCore => throw new NotImplementedException();
        protected override IDictionary<object, object> UserStateCore => _httpContext.Items;

        protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions options)
        {
            throw new NotImplementedException();
        }

        protected override async Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
        {
            // Headers can only be written once. Throw on subsequent call to write response header instead of silent no-op.
            if (_httpContext.Response.HasStarted)
            {
                throw new InvalidOperationException("Response headers can only be sent once per call.");
            }

            if (responseHeaders != null)
            {
                foreach (var entry in responseHeaders)
                {
                    if (entry.IsBinary)
                    {
                        _httpContext.Response.Headers[entry.Key] = Convert.ToBase64String(entry.ValueBytes);
                    }
                    else
                    {
                        _httpContext.Response.Headers[entry.Key] = entry.Value;
                    }
                }
            }

            await _httpContext.Response.BodyWriter.FlushAsync();
        }
    }
}
