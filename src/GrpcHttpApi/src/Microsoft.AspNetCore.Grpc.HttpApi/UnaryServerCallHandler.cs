// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Grpc.Core;
using Grpc.Gateway.Runtime;
using Grpc.Shared.HttpApi;
using Grpc.Shared.Server;
using Microsoft.AspNetCore.Grpc.HttpApi.Internal;
using Microsoft.AspNetCore.Grpc.HttpApi.Internal.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;

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
        private readonly List<FieldDescriptor>? _bodyFieldDescriptors;
        private readonly string? _bodyFieldDescriptorsPath;
        private readonly Dictionary<string, List<FieldDescriptor>> _routeParameterDescriptors;
        private readonly ConcurrentDictionary<string, List<FieldDescriptor>?> _pathDescriptorsCache;
        private readonly JsonSerializerOptions _options;

        [MemberNotNull(nameof(_bodyFieldDescriptors))]
        private bool BodyDescriptorRepeated { get; }

        public UnaryServerCallHandler(
            UnaryServerMethodInvoker<TService, TRequest, TResponse> unaryMethodInvoker,
            FieldDescriptor? responseBodyDescriptor,
            MessageDescriptor? bodyDescriptor,
            bool bodyDescriptorRepeated,
            List<FieldDescriptor>? bodyFieldDescriptors,
            Dictionary<string, List<FieldDescriptor>> routeParameterDescriptors,
            JsonSerializerOptions options)
        {
            _unaryMethodInvoker = unaryMethodInvoker;
            _responseBodyDescriptor = responseBodyDescriptor;
            _bodyDescriptor = bodyDescriptor;
            BodyDescriptorRepeated = bodyDescriptorRepeated;
            _bodyFieldDescriptors = bodyFieldDescriptors;
            if (_bodyFieldDescriptors != null)
            {
                _bodyFieldDescriptorsPath = string.Join('.', _bodyFieldDescriptors.Select(d => d.Name));
            }
            _routeParameterDescriptors = routeParameterDescriptors;
            _pathDescriptorsCache = new ConcurrentDictionary<string, List<FieldDescriptor>?>(StringComparer.Ordinal);
            _options = options;
        }

        public async Task HandleCallAsync(HttpContext httpContext)
        {
            var selectedEncoding = ResponseEncoding.SelectCharacterEncoding(httpContext.Request);

            var (requestMessage, requestStatusCode, errorMessage) = await CreateMessage(httpContext.Request);

            if (requestMessage == null || requestStatusCode != StatusCode.OK)
            {
                await SendErrorResponse(httpContext.Response, selectedEncoding, errorMessage ?? string.Empty, requestStatusCode);
                return;
            }

            var serverCallContext = new HttpApiServerCallContext(httpContext, _unaryMethodInvoker.Method.FullName);

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

        private async Task<(IMessage? requestMessage, StatusCode statusCode, string? errorMessage)> CreateMessage(HttpRequest request)
        {
            IMessage requestMessage;

            if (_bodyDescriptor != null)
            {
                if (!JsonRequestHelpers.HasJsonContentType(request, out var charset))
                {
                    return (null, StatusCode.InvalidArgument, "Request content-type of application/json is required.");
                }

                var encoding = JsonRequestHelpers.GetEncodingFromCharset(charset);
                var (stream, usesTranscodingStream) = JsonRequestHelpers.GetStream(request.HttpContext.Request.Body, encoding);

                try
                {
                    if (BodyDescriptorRepeated)
                    {
                        requestMessage = (IMessage)Activator.CreateInstance<TRequest>();
                        var repeatedContent = await ParseRepeatedContentAsync(stream);

                        ServiceDescriptorHelpers.RecursiveSetValue(requestMessage, _bodyFieldDescriptors, repeatedContent);
                    }
                    else
                    {
                        IMessage bodyContent;

                        try
                        {
                            bodyContent = (IMessage)(await JsonSerializer.DeserializeAsync(stream, _bodyDescriptor.ClrType, _options))!;
                        }
                        catch (JsonException)
                        {
                            return (null, StatusCode.InvalidArgument, "Request JSON payload is not correctly formatted.");
                        }
                        catch (Exception exception)
                        {
                            return (null, StatusCode.InvalidArgument, exception.Message);
                        }

                        if (_bodyFieldDescriptors != null)
                        {
                            requestMessage = (IMessage)Activator.CreateInstance<TRequest>();
                            ServiceDescriptorHelpers.RecursiveSetValue(requestMessage, _bodyFieldDescriptors, bodyContent!); // TODO - check nullability
                        }
                        else
                        {
                            requestMessage = bodyContent;
                        }
                    }
                }
                finally
                {
                    if (usesTranscodingStream)
                    {
                        await stream.DisposeAsync();
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

            return (requestMessage, StatusCode.OK, null);
        }

        private List<FieldDescriptor>? GetPathDescriptors(IMessage requestMessage, string path)
        {
            return _pathDescriptorsCache.GetOrAdd(path, p =>
            {
                ServiceDescriptorHelpers.TryResolveDescriptors(requestMessage.Descriptor, p, out var pathDescriptors);
                return pathDescriptors;
            });
        }

        private async ValueTask<IList> ParseRepeatedContentAsync(Stream inputStream)
        {
            var type = JsonConverterHelper.GetFieldType(_bodyFieldDescriptors!.Last());
            var listType = typeof(List<>).MakeGenericType(type);

            return (IList)(await JsonSerializer.DeserializeAsync(inputStream, listType, _options))!;
        }

        private async Task SendResponse(HttpResponse response, Encoding encoding, TResponse message)
        {
            object responseBody = message;

            if (_responseBodyDescriptor != null)
            {
                responseBody = _responseBodyDescriptor.Accessor.GetValue((IMessage)responseBody);
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.ContentType = MediaType.ReplaceEncoding("application/json", encoding);

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
            response.ContentType = MediaType.ReplaceEncoding("application/json", encoding);

            await WriteResponseMessage(response, encoding, e);
        }

        private async Task WriteResponseMessage(HttpResponse response, Encoding encoding, object responseBody)
        {
            var (stream, usesTranscodingStream) = JsonRequestHelpers.GetStream(response.Body, encoding);

            try
            {
                await JsonSerializer.SerializeAsync(stream, responseBody, _options);
            }
            finally
            {
                if (usesTranscodingStream)
                {
                    await stream.DisposeAsync();
                }
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
