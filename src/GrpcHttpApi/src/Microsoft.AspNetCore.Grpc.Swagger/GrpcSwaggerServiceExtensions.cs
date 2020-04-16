// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Grpc.Swagger.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for the gRPC HTTP API services.
    /// </summary>
    public static class GrpcSwaggerServiceExtensions
    {
        /// <summary>
        /// Adds gRPC HTTP API services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> for adding services.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddGrpcSwagger(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddGrpcHttpApi();

            services.TryAddEnumerable(ServiceDescriptor.Transient<IApiDescriptionProvider, GrpcHttpApiDescriptionProvider>());

            // Register default description provider in case MVC is not registered
            services.TryAddSingleton<IApiDescriptionGroupCollectionProvider>(serviceProvider =>
            {
                var actionDescriptorCollectionProvider = serviceProvider.GetService<IActionDescriptorCollectionProvider>();
                var apiDescriptionProvider = serviceProvider.GetServices<IApiDescriptionProvider>();

                return new ApiDescriptionGroupCollectionProvider(
                    actionDescriptorCollectionProvider ?? new EmptyActionDescriptorCollectionProvider(),
                    apiDescriptionProvider);
            });

            // Add or replace contract resolver.
            services.Replace(ServiceDescriptor.Transient<IDataContractResolver>(s =>
            {
                var serializerOptions = s.GetService<IOptions<JsonOptions>>()?.Value?.JsonSerializerOptions ?? new JsonSerializerOptions();
                var innerContractResolver = new JsonSerializerDataContractResolver(serializerOptions);
                return new GrpcDataContractResolver(innerContractResolver);
            }));

            return services;
        }

        // Dummy type that is only used if MVC is not registered in the app
        private class EmptyActionDescriptorCollectionProvider : IActionDescriptorCollectionProvider
        {
            public ActionDescriptorCollection ActionDescriptors { get; } = new ActionDescriptorCollection(new List<ActionDescriptor>(), 1);
        }
    }
}
