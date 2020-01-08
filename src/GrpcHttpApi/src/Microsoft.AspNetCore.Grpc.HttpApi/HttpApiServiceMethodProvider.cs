// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Google.Protobuf.Reflection;
using Grpc.AspNetCore.Server;
using Grpc.AspNetCore.Server.Model;
using Grpc.Shared.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Grpc.HttpApi
{
    internal class HttpApiServiceMethodProvider<TService> : IServiceMethodProvider<TService> where TService : class
    {
        private readonly ILogger<HttpApiServiceMethodProvider<TService>> _logger;
        private readonly GrpcServiceOptions _globalOptions;
        private readonly GrpcServiceOptions<TService> _serviceOptions;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly IGrpcServiceActivator<TService> _serviceActivator;

        public HttpApiServiceMethodProvider(
            ILoggerFactory loggerFactory,
            IOptions<GrpcServiceOptions> globalOptions,
            IOptions<GrpcServiceOptions<TService>> serviceOptions,
            IServiceProvider serviceProvider,
            IGrpcServiceActivator<TService> serviceActivator)
        {
            _logger = loggerFactory.CreateLogger<HttpApiServiceMethodProvider<TService>>();
            _globalOptions = globalOptions.Value;
            _serviceOptions = serviceOptions.Value;
            _loggerFactory = loggerFactory;
            _serviceProvider = serviceProvider;
            _serviceActivator = serviceActivator;
        }

        public void OnServiceMethodDiscovery(ServiceMethodProviderContext<TService> context)
        {
            var bindMethodInfo = BindMethodFinder.GetBindMethod(typeof(TService));

            // Invoke BindService(ServiceBinderBase, BaseType)
            if (bindMethodInfo != null)
            {
                // The second parameter is always the service base type
                var serviceParameter = bindMethodInfo.GetParameters()[1];

                ServiceDescriptor? serviceDescriptor = null;
                try
                {
                    serviceDescriptor = ServiceDescriptorHelpers.GetServiceDescriptor(bindMethodInfo.DeclaringType!);
                }
                catch (Exception ex)
                {
                    Log.ServiceDescriptorError(_logger, typeof(TService), ex);
                }

                if (serviceDescriptor != null)
                {
                    var binder = new HttpApiProviderServiceBinder<TService>(
                        context,
                        serviceParameter.ParameterType,
                        serviceDescriptor,
                        _globalOptions,
                        _serviceOptions,
                        _serviceProvider,
                        _loggerFactory,
                        _serviceActivator);

                    try
                    {
                        bindMethodInfo.Invoke(null, new object?[] { binder, null });
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Error binding gRPC service '{typeof(TService).Name}'.", ex);
                    }
                }
            }
            else
            {
                Log.BindMethodNotFound(_logger, typeof(TService));
            }
        }

        private static class Log
        {
            private static readonly Action<ILogger, Type, Exception?> _bindMethodNotFound =
                LoggerMessage.Define<Type>(LogLevel.Warning, new EventId(1, "BindMethodNotFound"), "Could not find bind method for {ServiceType}.");

            private static readonly Action<ILogger, Type, Exception> _serviceDescriptorError =
                LoggerMessage.Define<Type>(LogLevel.Warning, new EventId(2, "ServiceDescriptorError"), "Error getting service descriptor for {ServiceType}.");

            public static void BindMethodNotFound(ILogger logger, Type serviceType)
            {
                _bindMethodNotFound(logger, serviceType, null);
            }

            public static void ServiceDescriptorError(ILogger logger, Type serviceType, Exception ex)
            {
                _serviceDescriptorError(logger, serviceType, ex);
            }
        }
    }
}
