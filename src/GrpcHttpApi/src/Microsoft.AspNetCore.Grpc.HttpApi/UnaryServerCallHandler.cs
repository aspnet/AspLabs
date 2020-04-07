// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Gateway.Runtime;
using Grpc.Shared.HttpApi;
using Grpc.Shared.Server;
using Microsoft.AspNetCore.Grpc.HttpApi.Internal;
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
        private readonly bool _bodyDescriptorRepeated;
        private readonly List<FieldDescriptor>? _bodyFieldDescriptors;
        private readonly string? _bodyFieldDescriptorsPath;
        private readonly Dictionary<string, List<FieldDescriptor>> _routeParameterDescriptors;
        private readonly ConcurrentDictionary<string, List<FieldDescriptor>?> _pathDescriptorsCache;
        private readonly List<FieldDescriptor>? _resolvedBodyFieldDescriptors;

        public UnaryServerCallHandler(
            UnaryServerMethodInvoker<TService, TRequest, TResponse> unaryMethodInvoker,
            FieldDescriptor? responseBodyDescriptor,
            MessageDescriptor? bodyDescriptor,
            bool bodyDescriptorRepeated,
            List<FieldDescriptor>? bodyFieldDescriptors,
            Dictionary<string, List<FieldDescriptor>> routeParameterDescriptors)
        {
            _unaryMethodInvoker = unaryMethodInvoker;
            _responseBodyDescriptor = responseBodyDescriptor;
            _bodyDescriptor = bodyDescriptor;
            _bodyDescriptorRepeated = bodyDescriptorRepeated;
            _bodyFieldDescriptors = bodyFieldDescriptors;
            if (_bodyFieldDescriptors != null)
            {
                _bodyFieldDescriptorsPath = string.Join('.', _bodyFieldDescriptors.Select(d => d.Name));
            }
            if (_bodyDescriptorRepeated && _bodyFieldDescriptors != null)
            {
                _resolvedBodyFieldDescriptors = _bodyFieldDescriptors.Take(_bodyFieldDescriptors.Count - 1).ToList();
            }
            _routeParameterDescriptors = routeParameterDescriptors;
            _pathDescriptorsCache = new ConcurrentDictionary<string, List<FieldDescriptor>?>(StringComparer.Ordinal);
        }

        public async Task HandleCallAsync(HttpContext httpContext)
        {
            var requestMessage = await CreateMessage(httpContext.Request);

            var serverCallContext = new HttpApiServerCallContext(httpContext, _unaryMethodInvoker.Method.FullName);

            var selectedEncoding = ResponseEncoding.SelectCharacterEncoding(httpContext.Request);

            TResponse responseMessage;
            try
            {
                responseMessage = await _unaryMethodInvoker.Invoke(httpContext, serverCallContext, (TRequest)requestMessage);
            }
            catch (Exception ex)
            {
                StatusCode statusCode;
                string message;

                if (ex is RpcException rpcException)
                {
                    message = rpcException.Message;
                    statusCode = rpcException.StatusCode;
                }
                else
                {
                    // TODO - Add option for detailed error messages
                    message = "Exception was thrown by handler.";
                    statusCode = StatusCode.Unknown;
                }

                await SendErrorResponse(httpContext.Response, selectedEncoding, message, statusCode);
                return;
            }

            if (serverCallContext.Status.StatusCode != StatusCode.OK)
            {
                await SendErrorResponse(httpContext.Response, selectedEncoding, serverCallContext.Status.ToString(), serverCallContext.Status.StatusCode);
                return;
            }

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

                using (var requestReader = new HttpRequestStreamReader(request.Body, encoding))
                {
                    if (_bodyDescriptorRepeated)
                    {
                        var containingMessage = ParseRepeatedContent(requestReader);

                        if (_resolvedBodyFieldDescriptors!.Count > 0)
                        {
                            requestMessage = (IMessage)Activator.CreateInstance<TRequest>();
                            ServiceDescriptorHelpers.RecursiveSetValue(requestMessage, _resolvedBodyFieldDescriptors, containingMessage);
                        }
                        else
                        {
                            requestMessage = containingMessage;
                        }                        
                    }
                    else
                    {
                        var bodyContent = JsonParser.Default.Parse(requestReader, _bodyDescriptor);

                        if (_bodyFieldDescriptors != null)
                        {
                            requestMessage = (IMessage)Activator.CreateInstance<TRequest>();
                            ServiceDescriptorHelpers.RecursiveSetValue(requestMessage, _bodyFieldDescriptors, bodyContent);
                        }
                        else
                        {
                            requestMessage = bodyContent;
                        }
                    }
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
                    var pathDescriptors = GetPathDescriptors(requestMessage, item.Key);

                    if (pathDescriptors != null)
                    {
                        object value = item.Value.Count == 1 ? (object)item.Value[0] : item.Value;
                        ServiceDescriptorHelpers.RecursiveSetValue(requestMessage, pathDescriptors, value);
                    }
                }
            }

            return requestMessage;
        }

        private List<FieldDescriptor>? GetPathDescriptors(IMessage requestMessage, string path)
        {
            return _pathDescriptorsCache.GetOrAdd(path, p =>
            {
                ServiceDescriptorHelpers.TryResolveDescriptors(requestMessage.Descriptor, p, out var pathDescriptors);
                return pathDescriptors;
            });
        }

        private IMessage ParseRepeatedContent(HttpRequestStreamReader requestReader)
        {
            // The following code is SUPER hacky.
            //
            // Problem:
            // JsonParser doesn't provide a way to directly parse a JSON array to repeated fields.
            // JsonParser's Parse methods only support reading JSON objects as methods.
            //
            // Solution:
            // To get around this limitation a wrapping TextReader is created that inserts a wrapping
            // object into the JSON passed to the parser. The parser returns the containing message
            // with the repeated fields set on it.
            var containingType = _bodyFieldDescriptors.Last()!.ContainingType;

            return JsonParser.Default.Parse(new PropertyWrappingTextReader(requestReader, _bodyFieldDescriptors.Last().JsonName), containingType);
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

            await WriteResponseMessage(response, encoding, responseBody);
        }

        private async Task SendErrorResponse(HttpResponse response, Encoding encoding, string message, StatusCode statusCode)
        {
            var e = new Error
            {
                Error_ = message,
                Message = message,
                Code = (int)statusCode
            };

            response.StatusCode = MapStatusCodeToHttpStatus(statusCode);
            response.ContentType = "application/json";

            await WriteResponseMessage(response, encoding, e);
        }

        private static async Task WriteResponseMessage(HttpResponse response, Encoding encoding, object responseBody)
        {
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

        private static int MapStatusCodeToHttpStatus(StatusCode statusCode)
        {
            switch (statusCode)
            {
                case StatusCode.OK:
                    return StatusCodes.Status200OK;
                case StatusCode.Cancelled:
                    return StatusCodes.Status408RequestTimeout;
                case StatusCode.Unknown:
                    return StatusCodes.Status500InternalServerError;
                case StatusCode.InvalidArgument:
                    return StatusCodes.Status400BadRequest;
                case StatusCode.DeadlineExceeded:
                    return StatusCodes.Status504GatewayTimeout;
                case StatusCode.NotFound:
                    return StatusCodes.Status404NotFound;
                case StatusCode.AlreadyExists:
                    return StatusCodes.Status409Conflict;
                case StatusCode.PermissionDenied:
                    return StatusCodes.Status403Forbidden;
                case StatusCode.Unauthenticated:
                    return StatusCodes.Status401Unauthorized;
                case StatusCode.ResourceExhausted:
                    return StatusCodes.Status429TooManyRequests;
                case StatusCode.FailedPrecondition:
                    // Note, this deliberately doesn't translate to the similarly named '412 Precondition Failed' HTTP response status.
                    return StatusCodes.Status400BadRequest;
                case StatusCode.Aborted:
                    return StatusCodes.Status409Conflict;
                case StatusCode.OutOfRange:
                    return StatusCodes.Status400BadRequest;
                case StatusCode.Unimplemented:
                    return StatusCodes.Status501NotImplemented;
                case StatusCode.Internal:
                    return StatusCodes.Status500InternalServerError;
                case StatusCode.Unavailable:
                    return StatusCodes.Status503ServiceUnavailable;
                case StatusCode.DataLoss:
                    return StatusCodes.Status500InternalServerError;
            }

            return StatusCodes.Status500InternalServerError;
        }

        private bool CanBindQueryStringVariable(string variable)
        {
            if (_bodyDescriptor != null)
            {
                if (_bodyFieldDescriptors == null || _bodyFieldDescriptors.Count == 0)
                {
                    return false;
                }

                if (variable == _bodyFieldDescriptorsPath)
                {
                    return false;
                }

                if (variable.StartsWith(_bodyFieldDescriptorsPath!, StringComparison.Ordinal))
                {
                    return false;
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
