// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Google.Api;
using Google.Protobuf.Reflection;
using Grpc.AspNetCore.Server;
using Grpc.AspNetCore.Server.Model;
using Grpc.Core;
using Grpc.Shared.HttpApi;
using Grpc.Shared.Server;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Grpc.HttpApi
{
    internal class HttpApiProviderServiceBinder<TService> : ServiceBinderBase where TService : class
    {
        private readonly ServiceMethodProviderContext<TService> _context;
        private readonly Type _declaringType;
        private readonly ServiceDescriptor _serviceDescriptor;
        private readonly GrpcServiceOptions _globalOptions;
        private readonly GrpcServiceOptions<TService> _serviceOptions;
        private readonly IGrpcServiceActivator<TService> _serviceActivator;
        private readonly GrpcHttpApiOptions _httpApiOptions;
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly ILogger _logger;

        internal HttpApiProviderServiceBinder(
            ServiceMethodProviderContext<TService> context,
            Type declaringType,
            ServiceDescriptor serviceDescriptor,
            GrpcServiceOptions globalOptions,
            GrpcServiceOptions<TService> serviceOptions,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            IGrpcServiceActivator<TService> serviceActivator,
            GrpcHttpApiOptions httpApiOptions,
            JsonSerializerOptions serializerOptions)
        {
            _context = context;
            _declaringType = declaringType;
            _serviceDescriptor = serviceDescriptor;
            _globalOptions = globalOptions;
            _serviceOptions = serviceOptions;
            _serviceActivator = serviceActivator;
            _httpApiOptions = httpApiOptions;
            _serializerOptions = serializerOptions;
            _logger = loggerFactory.CreateLogger<HttpApiProviderServiceBinder<TService>>();
        }

        public override void AddMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ClientStreamingServerMethod<TRequest, TResponse> handler)
        {
            if (TryGetMethodDescriptor(method.Name, out var methodDescriptor) &&
                ServiceDescriptorHelpers.TryGetHttpRule(methodDescriptor, out _))
            {
                Log.StreamingMethodNotSupported(_logger, method.Name, typeof(TService));
            }
        }

        public override void AddMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, DuplexStreamingServerMethod<TRequest, TResponse> handler)
        {
            if (TryGetMethodDescriptor(method.Name, out var methodDescriptor) &&
                ServiceDescriptorHelpers.TryGetHttpRule(methodDescriptor, out _))
            {
                Log.StreamingMethodNotSupported(_logger, method.Name, typeof(TService));
            }
        }

        public override void AddMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ServerStreamingServerMethod<TRequest, TResponse> handler)
        {
            if (TryGetMethodDescriptor(method.Name, out var methodDescriptor) &&
                ServiceDescriptorHelpers.TryGetHttpRule(methodDescriptor, out _))
            {
                Log.StreamingMethodNotSupported(_logger, method.Name, typeof(TService));
            }
        }

        public override void AddMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, UnaryServerMethod<TRequest, TResponse> handler)
        {
            if (TryGetMethodDescriptor(method.Name, out var methodDescriptor))
            {
                if (ServiceDescriptorHelpers.TryGetHttpRule(methodDescriptor, out var httpRule))
                {
                    ProcessHttpRule(method, methodDescriptor, httpRule);
                }
                else
                {
                    // Consider setting to enable mapping to methods without HttpRule
                    // AddMethodCore(method, method.FullName, "GET", string.Empty, string.Empty, methodDescriptor);
                }
            }
            else
            {
                Log.MethodDescriptorNotFound(_logger, method.Name, typeof(TService));
            }
        }

        private void ProcessHttpRule<TRequest, TResponse>(Method<TRequest, TResponse> method, MethodDescriptor methodDescriptor, HttpRule httpRule)
            where TRequest : class
            where TResponse : class
        {
            if (ServiceDescriptorHelpers.TryResolvePattern(httpRule, out var pattern, out var httpVerb))
            {
                AddMethodCore(method, httpRule, pattern, httpVerb, httpRule.Body, httpRule.ResponseBody, methodDescriptor);
            }

            foreach (var additionalRule in httpRule.AdditionalBindings)
            {
                ProcessHttpRule(method, methodDescriptor, additionalRule);
            }
        }

        private void AddMethodCore<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            HttpRule httpRule,
            string pattern,
            string httpVerb,
            string body,
            string responseBody,
            MethodDescriptor methodDescriptor)
            where TRequest : class
            where TResponse : class
        {
            try
            {
                if (!pattern.StartsWith('/'))
                {
                    // This validation is consistent with grpc-gateway code generation.
                    // We should match their validation to be a good member of the eco-system.
                    throw new InvalidOperationException($"Path template must start with /: {pattern}");
                }

                var (invoker, metadata) = CreateModelCore<UnaryServerMethod<TService, TRequest, TResponse>>(
                    method.Name,
                    new[] { typeof(TRequest), typeof(ServerCallContext) },
                    httpVerb,
                    httpRule,
                    methodDescriptor);

                var methodContext = global::Grpc.Shared.Server.MethodOptions.Create(new[] { _globalOptions, _serviceOptions });

                var routePattern = RoutePatternFactory.Parse(pattern);
                var routeParameterDescriptors = ServiceDescriptorHelpers.ResolveRouteParameterDescriptors(routePattern, methodDescriptor.InputType);

                var bodyDescriptor = ServiceDescriptorHelpers.ResolveBodyDescriptor(body, typeof(TService), methodDescriptor);

                FieldDescriptor? responseBodyDescriptor = null;
                if (!string.IsNullOrEmpty(responseBody))
                {
                    responseBodyDescriptor = methodDescriptor.OutputType.FindFieldByName(responseBody);
                    if (responseBodyDescriptor == null)
                    {
                        throw new InvalidOperationException($"Couldn't find matching field for response body '{responseBody}' on {methodDescriptor.OutputType.Name}.");
                    }
                }

                var unaryInvoker = new UnaryServerMethodInvoker<TService, TRequest, TResponse>(invoker, method, methodContext, _serviceActivator);
                var unaryServerCallHandler = new UnaryServerCallHandler<TService, TRequest, TResponse>(
                    unaryInvoker,
                    responseBodyDescriptor,
                    bodyDescriptor?.Descriptor,
                    bodyDescriptor?.IsDescriptorRepeated ?? false,
                    bodyDescriptor?.FieldDescriptors,
                    routeParameterDescriptors,
                    _serializerOptions);

                _context.AddMethod<TRequest, TResponse>(method, routePattern, metadata, unaryServerCallHandler.HandleCallAsync);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error binding {method.Name} on {typeof(TService).Name} to HTTP API.", ex);
            }
        }

        private bool TryGetMethodDescriptor(string methodName, [NotNullWhen(true)]out MethodDescriptor? methodDescriptor)
        {
            methodDescriptor = _serviceDescriptor.Methods.SingleOrDefault(m => m.Name == methodName);
            return (methodDescriptor != null);
        }

        private (TDelegate invoker, List<object> metadata) CreateModelCore<TDelegate>(string methodName, Type[] methodParameters, string verb, HttpRule httpRule, MethodDescriptor methodDescriptor) where TDelegate : Delegate
        {
            var handlerMethod = GetMethod(methodName, methodParameters);

            if (handlerMethod == null)
            {
                throw new InvalidOperationException($"Could not find '{methodName}' on {typeof(TService)}.");
            }

            var invoker = (TDelegate)Delegate.CreateDelegate(typeof(TDelegate), handlerMethod);

            var metadata = new List<object>();
            // Add type metadata first so it has a lower priority
            metadata.AddRange(typeof(TService).GetCustomAttributes(inherit: true));
            // Add method metadata last so it has a higher priority
            metadata.AddRange(handlerMethod.GetCustomAttributes(inherit: true));
            metadata.Add(new HttpMethodMetadata(new[] { verb }));

            // Add protobuf service method descriptor.
            // Is used by swagger generation to identify gRPC HTTP APIs.
            metadata.Add(new GrpcHttpMetadata(methodDescriptor, httpRule));

            return (invoker, metadata);
        }

        private MethodInfo? GetMethod(string methodName, Type[] methodParameters)
        {
            Type? currentType = typeof(TService);
            while (currentType != null)
            {
                var matchingMethod = currentType.GetMethod(
                    methodName,
                    BindingFlags.Public | BindingFlags.Instance,
                    binder: null,
                    types: methodParameters,
                    modifiers: null);

                if (matchingMethod == null)
                {
                    return null;
                }

                // Validate that the method overrides the virtual method on the base service type.
                // If there is a method with the same name it will hide the base method. Ignore it,
                // and continue searching on the base type.
                if (matchingMethod.IsVirtual)
                {
                    var baseDefinitionMethod = matchingMethod.GetBaseDefinition();
                    if (baseDefinitionMethod != null && baseDefinitionMethod.DeclaringType == _declaringType)
                    {
                        return matchingMethod;
                    }
                }

                currentType = currentType.BaseType;
            }

            return null;
        }

        private static class Log
        {
            private static readonly Action<ILogger, string, Type, Exception?> _streamingMethodNotSupported =
                LoggerMessage.Define<string, Type>(LogLevel.Warning, new EventId(1, "StreamingMethodNotSupported"), "Unable to bind {MethodName} on {ServiceType} to HTTP API. Streaming methods are not supported.");

            private static readonly Action<ILogger, string, Type, Exception?> _methodDescriptorNotFound =
                LoggerMessage.Define<string, Type>(LogLevel.Warning, new EventId(2, "MethodDescriptorNotFound"), "Unable to find method descriptor for {MethodName} on {ServiceType}.");

            public static void StreamingMethodNotSupported(ILogger logger, string methodName, Type serviceType)
            {
                _streamingMethodNotSupported(logger, methodName, serviceType, null);
            }

            public static void MethodDescriptorNotFound(ILogger logger, string methodName, Type serviceType)
            {
                _methodDescriptorNotFound(logger, methodName, serviceType, null);
            }
        }
    }
}
