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
using Microsoft.AspNetCore.Grpc.HttpApi.Internal.CallHandlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Logging;
using MethodOptions = global::Grpc.Shared.Server.MethodOptions;

namespace Microsoft.AspNetCore.Grpc.HttpApi
{
    internal interface IServiceInvokerResolver<TService> where TService : class
    {
        (TDelegate invoker, List<object> metadata) CreateModelCore<TDelegate>(
            string methodName,
            Type[] methodParameters,
            string verb,
            HttpRule httpRule,
            MethodDescriptor methodDescriptor) where TDelegate : Delegate;
    }

    internal class ReflectionServiceInvokerResolver<TService>
        : IServiceInvokerResolver<TService> where TService : class
    {
        private readonly Type _declaringType;

        public ReflectionServiceInvokerResolver(Type declaringType)
        {
            _declaringType = declaringType;
        }

        public (TDelegate invoker, List<object> metadata) CreateModelCore<TDelegate>(
            string methodName,
            Type[] methodParameters,
            string verb,
            HttpRule httpRule,
            MethodDescriptor methodDescriptor) where TDelegate : Delegate
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
    }

    internal class HttpApiProviderServiceBinder<TService> : ServiceBinderBase where TService : class
    {
        private delegate (RequestDelegate RequestDelegate, List<object> Metadata) CreateRequestDelegate<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            string httpVerb,
            HttpRule httpRule,
            MethodDescriptor methodDescriptor,
            CallHandlerDescriptorInfo descriptorInfo,
            MethodOptions methodOptions);

        private readonly ServiceMethodProviderContext<TService> _context;
        private readonly IServiceInvokerResolver<TService> _invokerResolver;
        private readonly ServiceDescriptor _serviceDescriptor;
        private readonly GrpcServiceOptions _globalOptions;
        private readonly GrpcServiceOptions<TService> _serviceOptions;
        private readonly IGrpcServiceActivator<TService> _serviceActivator;
        private readonly GrpcHttpApiOptions _httpApiOptions;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        internal HttpApiProviderServiceBinder(
            ServiceMethodProviderContext<TService> context,
            IServiceInvokerResolver<TService> invokerResolver,
            ServiceDescriptor serviceDescriptor,
            GrpcServiceOptions globalOptions,
            GrpcServiceOptions<TService> serviceOptions,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            IGrpcServiceActivator<TService> serviceActivator,
            GrpcHttpApiOptions httpApiOptions)
        {
            _context = context;
            _invokerResolver = invokerResolver;
            _serviceDescriptor = serviceDescriptor;
            _globalOptions = globalOptions;
            _serviceOptions = serviceOptions;
            _serviceActivator = serviceActivator;
            _httpApiOptions = httpApiOptions;
            _loggerFactory = loggerFactory;
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
            if (TryGetMethodDescriptor(method.Name, out var methodDescriptor))
            {
                if (ServiceDescriptorHelpers.TryGetHttpRule(methodDescriptor, out var httpRule))
                {
                    ProcessHttpRule(method, methodDescriptor, httpRule, CreateServerStreamingRequestDelegate);
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

        public override void AddMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, UnaryServerMethod<TRequest, TResponse> handler)
        {
            if (TryGetMethodDescriptor(method.Name, out var methodDescriptor))
            {
                if (ServiceDescriptorHelpers.TryGetHttpRule(methodDescriptor, out var httpRule))
                {
                    ProcessHttpRule(method, methodDescriptor, httpRule, CreateUnaryRequestDelegate);
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

        private void ProcessHttpRule<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            MethodDescriptor methodDescriptor,
            HttpRule httpRule,
            CreateRequestDelegate<TRequest, TResponse> createRequestDelegate)
            where TRequest : class
            where TResponse : class
        {
            if (ServiceDescriptorHelpers.TryResolvePattern(httpRule, out var pattern, out var httpVerb))
            {
                AddMethodCore(method, httpRule, pattern, httpVerb, httpRule.Body, httpRule.ResponseBody, methodDescriptor, createRequestDelegate);
            }

            foreach (var additionalRule in httpRule.AdditionalBindings)
            {
                ProcessHttpRule(method, methodDescriptor, additionalRule, createRequestDelegate);
            }
        }

        private (RequestDelegate RequestDelegate, List<object> Metadata) CreateUnaryRequestDelegate<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            string httpVerb,
            HttpRule httpRule,
            MethodDescriptor methodDescriptor,
            CallHandlerDescriptorInfo descriptorInfo,
            MethodOptions methodOptions)
            where TRequest : class
            where TResponse : class
        {
            var (invoker, metadata) = _invokerResolver.CreateModelCore<UnaryServerMethod<TService, TRequest, TResponse>>(
                method.Name,
                new[] { typeof(TRequest), typeof(ServerCallContext) },
                httpVerb,
                httpRule,
                methodDescriptor);

            var methodInvoker = new UnaryServerMethodInvoker<TService, TRequest, TResponse>(invoker, method, methodOptions, _serviceActivator);
            var callHandler = new UnaryServerCallHandler<TService, TRequest, TResponse>(
                methodInvoker,
                _loggerFactory,
                descriptorInfo,
                _httpApiOptions.JsonSettings.UnarySerializerOptions);

            return (callHandler.HandleCallAsync, metadata);
        }

        private (RequestDelegate RequestDelegate, List<object> Metadata) CreateServerStreamingRequestDelegate<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            string httpVerb,
            HttpRule httpRule,
            MethodDescriptor methodDescriptor,
            CallHandlerDescriptorInfo descriptorInfo,
            MethodOptions methodOptions)
            where TRequest : class
            where TResponse : class
        {
            var (invoker, metadata) = _invokerResolver.CreateModelCore<ServerStreamingServerMethod<TService, TRequest, TResponse>>(
                method.Name,
                new[] { typeof(TRequest), typeof(IServerStreamWriter<TResponse>), typeof(ServerCallContext) },
                httpVerb,
                httpRule,
                methodDescriptor);

            var methodInvoker = new ServerStreamingServerMethodInvoker<TService, TRequest, TResponse>(invoker, method, methodOptions, _serviceActivator);
            var callHandler = new ServerStreamingServerCallHandler<TService, TRequest, TResponse>(
                methodInvoker,
                _loggerFactory,
                descriptorInfo,
                _httpApiOptions.JsonSettings.ServerStreamingSerializerOptions);

            return (callHandler.HandleCallAsync, metadata);
        }

        private void AddMethodCore<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            HttpRule httpRule,
            string pattern,
            string httpVerb,
            string body,
            string responseBody,
            MethodDescriptor methodDescriptor,
            CreateRequestDelegate<TRequest, TResponse> createRequestDelegate)
            where TRequest : class
            where TResponse : class
        {
            try
            {
                var (routePattern, descriptorInfo) = ParseRoute(pattern, body, responseBody, methodDescriptor);
                var methodOptions = MethodOptions.Create(new[] { _globalOptions, _serviceOptions });

                var (requestDelegate, metadata) = createRequestDelegate(method, httpVerb, httpRule, methodDescriptor, descriptorInfo, methodOptions);

                _context.AddMethod<TRequest, TResponse>(method, routePattern, metadata, requestDelegate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error binding {method.Name} on {typeof(TService).Name} to HTTP API.", ex);
            }
        }

        private static (RoutePattern routePattern, CallHandlerDescriptorInfo descriptorInfo) ParseRoute(string pattern, string body, string responseBody, MethodDescriptor methodDescriptor)
        {
            if (!pattern.StartsWith('/'))
            {
                // This validation is consistent with grpc-gateway code generation.
                // We should match their validation to be a good member of the eco-system.
                throw new InvalidOperationException($"Path template must start with /: {pattern}");
            }

            var routePattern = RoutePatternFactory.Parse(pattern);
            return (RoutePatternFactory.Parse(pattern), CreateDescriptorInfo(body, responseBody, methodDescriptor, routePattern));
        }

        private static CallHandlerDescriptorInfo CreateDescriptorInfo(string body, string responseBody, MethodDescriptor methodDescriptor, RoutePattern routePattern)
        {
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

            var descriptorInfo = new CallHandlerDescriptorInfo(
                responseBodyDescriptor,
                bodyDescriptor?.Descriptor,
                bodyDescriptor?.IsDescriptorRepeated ?? false,
                bodyDescriptor?.FieldDescriptors,
                routeParameterDescriptors);
            return descriptorInfo;
        }

        private bool TryGetMethodDescriptor(string methodName, [NotNullWhen(true)]out MethodDescriptor? methodDescriptor)
        {
            methodDescriptor = _serviceDescriptor.Methods.SingleOrDefault(m => m.Name == methodName);
            return (methodDescriptor != null);
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
