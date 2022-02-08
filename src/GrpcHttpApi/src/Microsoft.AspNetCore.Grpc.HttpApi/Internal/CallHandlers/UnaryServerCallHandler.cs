// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
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
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Internal.CallHandlers
{
    internal class UnaryServerCallHandler<TService, TRequest, TResponse> : ServerCallHandlerBase<TService, TRequest, TResponse>
        where TService : class
        where TRequest : class
        where TResponse : class
    {
        private readonly UnaryServerMethodInvoker<TService, TRequest, TResponse> _unaryMethodInvoker;
        private readonly CallHandlerDescriptorInfo _descriptorInfo;

        public UnaryServerCallHandler(
            UnaryServerMethodInvoker<TService, TRequest, TResponse> unaryMethodInvoker,
            ILoggerFactory loggerFactory,
            CallHandlerDescriptorInfo descriptorInfo,
            JsonSerializerOptions options) : base(unaryMethodInvoker, loggerFactory, options)
        {
            _unaryMethodInvoker = unaryMethodInvoker;
            _descriptorInfo = descriptorInfo;
        }

        protected override async Task HandleCallAsyncCore(HttpContext httpContext, HttpApiServerCallContext serverCallContext)
        {
            var requestMessage = (TRequest)await CreateMessage(serverCallContext);

            var responseMessage = await _unaryMethodInvoker.Invoke(httpContext, serverCallContext, requestMessage);
            if (serverCallContext.Status.StatusCode != StatusCode.OK)
            {
                throw new RpcException(serverCallContext.Status);
            }

            GrpcServerLog.SendingMessage(Logger);

            await SendResponse(httpContext.Response, serverCallContext.RequestEncoding, responseMessage);
        }

        private async Task<IMessage> CreateMessage(HttpApiServerCallContext serverCallContext)
        {
            IMessage requestMessage;

            if (_descriptorInfo.BodyDescriptor != null)
            {
                if (!serverCallContext.IsJsonRequestContent)
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Request content-type of application/json is required."));
                }

                var (stream, usesTranscodingStream) = JsonRequestHelpers.GetStream(serverCallContext.HttpContext.Request.Body, serverCallContext.RequestEncoding);

                try
                {
                    if (_descriptorInfo.BodyDescriptorRepeated)
                    {
                        requestMessage = (IMessage)Activator.CreateInstance<TRequest>();
                        var repeatedContent = await ParseRepeatedContentAsync(stream);

                        ServiceDescriptorHelpers.RecursiveSetValue(requestMessage, _descriptorInfo.BodyFieldDescriptors!, repeatedContent);
                    }
                    else
                    {
                        IMessage bodyContent;

                        try
                        {
                            bodyContent = (IMessage)(await JsonSerializer.DeserializeAsync(stream, _descriptorInfo.BodyDescriptor.ClrType, SerializerOptions))!;
                        }
                        catch (JsonException)
                        {
                            throw new RpcException(new Status(StatusCode.InvalidArgument, "Request JSON payload is not correctly formatted."));
                        }
                        catch (Exception exception)
                        {
                            throw new RpcException(new Status(StatusCode.InvalidArgument, exception.Message));
                        }

                        if (_descriptorInfo.BodyFieldDescriptors != null)
                        {
                            requestMessage = (IMessage)Activator.CreateInstance<TRequest>();
                            ServiceDescriptorHelpers.RecursiveSetValue(requestMessage, _descriptorInfo.BodyFieldDescriptors, bodyContent!); // TODO - check nullability
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

            foreach (var parameterDescriptor in _descriptorInfo.RouteParameterDescriptors)
            {
                var routeValue = serverCallContext.HttpContext.Request.RouteValues[parameterDescriptor.Key];
                if (routeValue != null)
                {
                    ServiceDescriptorHelpers.RecursiveSetValue(requestMessage, parameterDescriptor.Value, routeValue);
                }
            }

            foreach (var item in serverCallContext.HttpContext.Request.Query)
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
            return _descriptorInfo.PathDescriptorsCache.GetOrAdd(path, p =>
            {
                ServiceDescriptorHelpers.TryResolveDescriptors(requestMessage.Descriptor, p, out var pathDescriptors);
                return pathDescriptors;
            });
        }

        private async ValueTask<IList> ParseRepeatedContentAsync(Stream inputStream)
        {
            var type = JsonConverterHelper.GetFieldType(_descriptorInfo.BodyFieldDescriptors!.Last());
            var listType = typeof(List<>).MakeGenericType(type);

            return (IList)(await JsonSerializer.DeserializeAsync(inputStream, listType, SerializerOptions))!;
        }

        private async Task SendResponse(HttpResponse response, Encoding encoding, TResponse message)
        {
            object responseBody = message;

            if (_descriptorInfo.ResponseBodyDescriptor != null)
            {
                responseBody = _descriptorInfo.ResponseBodyDescriptor.Accessor.GetValue((IMessage)responseBody);
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.ContentType = MediaType.ReplaceEncoding("application/json", encoding);

            await JsonRequestHelpers.WriteResponseMessage(response, encoding, responseBody, SerializerOptions);
        }

        private bool CanBindQueryStringVariable(string variable)
        {
            if (_descriptorInfo.BodyDescriptor != null)
            {
                if (_descriptorInfo.BodyFieldDescriptors == null || _descriptorInfo.BodyFieldDescriptors.Count == 0)
                {
                    return false;
                }

                if (variable == _descriptorInfo.BodyFieldDescriptorsPath)
                {
                    return false;
                }

                if (variable.StartsWith(_descriptorInfo.BodyFieldDescriptorsPath!, StringComparison.Ordinal))
                {
                    return false;
                }
            }

            if (_descriptorInfo.RouteParameterDescriptors.ContainsKey(variable))
            {
                return false;
            }

            return true;
        }
    }
}
