// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Grpc.Shared.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace Microsoft.AspNetCore.Grpc.HttpApi
{
    internal class UnaryServerCallHandler<TService, TRequest, TResponse>
        where TService : class
        where TRequest : class
        where TResponse : class
    {
        private readonly UnaryServerMethodInvoker<TService, TRequest, TResponse> _unaryMethodInvoker;
        private readonly FieldDescriptor? _responseBodyDescriptor;
        private readonly MessageDescriptor? _bodyDescriptor;
        private readonly FieldDescriptor? _bodyFieldDescriptor;
        private readonly Dictionary<string, List<FieldDescriptor>> _routeParameterDescriptors;
        private readonly ConcurrentDictionary<string, List<FieldDescriptor>> _queryParameterDescriptors;

        public UnaryServerCallHandler(
            UnaryServerMethodInvoker<TService, TRequest, TResponse> unaryMethodInvoker,
            FieldDescriptor? responseBodyDescriptor,
            MessageDescriptor? bodyDescriptor,
            FieldDescriptor? bodyFieldDescriptor,
            Dictionary<string, List<FieldDescriptor>> routeParameterDescriptors)
        {
            _unaryMethodInvoker = unaryMethodInvoker;
            _responseBodyDescriptor = responseBodyDescriptor;
            _bodyDescriptor = bodyDescriptor;
            _bodyFieldDescriptor = bodyFieldDescriptor;
            _routeParameterDescriptors = routeParameterDescriptors;
            _queryParameterDescriptors = new ConcurrentDictionary<string, List<FieldDescriptor>>(StringComparer.Ordinal);
        }

        public async Task HandleCallAsync(HttpContext httpContext)
        {
            var requestMessage = await CreateMessage(httpContext.Request);

            var serverCallContext = new HttpApiServerCallContext();

            var responseMessage = await _unaryMethodInvoker.Invoke(httpContext, serverCallContext, (TRequest)requestMessage);

            var selectedEncoding = ResponseEncoding.SelectCharacterEncoding(httpContext.Request);
            await SendResponse(httpContext.Response, selectedEncoding, responseMessage);
        }

        private async Task<IMessage> CreateMessage(HttpRequest request)
        {
            IMessage? requestMessage;

            if (_bodyDescriptor != null)
            {
                if (request.ContentType == null ||
                    !request.ContentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Request content-type of application/json is required.");
                }

                if (!request.Body.CanSeek)
                {
                    // JsonParser does synchronous reads. In order to avoid blocking on the stream, we asynchronously
                    // read everything into a buffer, and then seek back to the beginning.
                    request.EnableBuffering();
                    Debug.Assert(request.Body.CanSeek);

                    await request.Body.DrainAsync(CancellationToken.None);
                    request.Body.Seek(0L, SeekOrigin.Begin);
                }

                var encoding = RequestEncoding.SelectCharacterEncoding(request);
                // TODO: Handle unsupported encoding

                IMessage bodyContent;
                using (var requestReader = new HttpRequestStreamReader(request.Body, encoding))
                {
                    bodyContent = JsonParser.Default.Parse(requestReader, _bodyDescriptor);
                }

                if (_bodyFieldDescriptor != null)
                {
                    requestMessage = (IMessage)Activator.CreateInstance<TRequest>();
                    _bodyFieldDescriptor.Accessor.SetValue(requestMessage, bodyContent);
                }
                else
                {
                    requestMessage = bodyContent;
                }
            }
            else
            {
                requestMessage = (IMessage)Activator.CreateInstance<TRequest>();
            }

            foreach (var parameterDescriptor in _routeParameterDescriptors)
            {
                var routeValue = request.RouteValues[parameterDescriptor.Key];
                if (routeValue != null)
                {
                    ServiceDescriptorHelpers.RecursiveSetValue(requestMessage, parameterDescriptor.Value, routeValue);
                }
            }

            foreach (var item in request.Query)
            {
                if (CanBindQueryStringVariable(item.Key))
                {
                    if (!_queryParameterDescriptors.TryGetValue(item.Key, out var pathDescriptors))
                    {
                        if (ServiceDescriptorHelpers.TryResolveDescriptors(requestMessage.Descriptor, item.Key, out pathDescriptors))
                        {
                            _queryParameterDescriptors[item.Key] = pathDescriptors;
                        }
                    }

                    if (pathDescriptors != null)
                    {
                        object value = item.Value.Count == 1 ? (object)item.Value[0] : item.Value;
                        ServiceDescriptorHelpers.RecursiveSetValue(requestMessage, pathDescriptors, value);
                    }
                }
            }

            return requestMessage;
        }

        private async Task SendResponse(HttpResponse response, Encoding encoding, TResponse message)
        {
            object responseBody = message;

            if (_responseBodyDescriptor != null)
            {
                responseBody = _responseBodyDescriptor.Accessor.GetValue((IMessage)responseBody);
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.ContentType = "application/json";

            using (var writer = new HttpResponseStreamWriter(response.Body, encoding))
            {
                if (responseBody is IMessage responseMessage)
                {
                    JsonFormatter.Default.Format(responseMessage, writer);
                }
                else
                {
                    JsonFormatter.Default.WriteValue(writer, responseBody);
                }

                // Perf: call FlushAsync to call WriteAsync on the stream with any content left in the TextWriter's
                // buffers. This is better than just letting dispose handle it (which would result in a synchronous
                // write).
                await writer.FlushAsync();
            }
        }

        private bool CanBindQueryStringVariable(string variable)
        {
            if (_bodyDescriptor != null)
            {
                if (_bodyFieldDescriptor == null)
                {
                    return false;
                }

                if (variable == _bodyFieldDescriptor.Name)
                {
                    return false;
                }

                var separator = variable.IndexOf('.', StringComparison.Ordinal);
                if (separator > -1)
                {
                    if (variable.AsSpan(0, separator).Equals(_bodyFieldDescriptor.Name.AsSpan(), StringComparison.Ordinal))
                    {
                        return false;
                    }
                }
            }

            if (_routeParameterDescriptors.ContainsKey(variable))
            {
                return false;
            }

            return true;
        }
    }
}
