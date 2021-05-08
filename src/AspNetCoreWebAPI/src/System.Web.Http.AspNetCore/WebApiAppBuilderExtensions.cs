// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Web.Http.Controllers;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Hosting;
using Microsoft.AspNetCore.Builder;

namespace System.Web.Http.AspNetCore
{
    /// <summary>
    /// Provides extension methods for the <see cref="IApplicationBuilder"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class WebApiAppBuilderExtensions
    {
        /// <summary>Adds a component to the OWIN pipeline for running a Web API endpoint.</summary>
        /// <param name="builder">The application builder.</param>
        /// <param name="configuration">The <see cref="HttpConfiguration"/> used to configure the endpoint.</param>
        /// <param name="bufferRequests">
        /// The default WebAPI formatters perform synchronous I/O when reading to the request.
        /// This has poor performance characteristics with ASP.NET Core apps and may result
        /// in application deadlocks. This option forces ASP.NET Core to buffer requests preventing this scenario.
        /// </param>
        /// <returns>The application builder.</returns>
        public static IApplicationBuilder UseWebApi(this IApplicationBuilder builder, HttpConfiguration configuration, bool bufferRequests = true)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            HttpServer server = new HttpServer(configuration);

            try
            {
                HttpMessageHandlerOptions options = CreateOptions(builder, server, configuration, bufferRequests);
                return UseMessageHandler(builder, options);
            }
            catch
            {
                server.Dispose();
                throw;
            }
        }

        /// <summary>Adds a component to the OWIN pipeline for running a Web API endpoint.</summary>
        /// <param name="builder">The application builder.</param>
        /// <param name="httpServer">The http server.</param>
        /// <param name="bufferRequests">
        /// The default WebAPI formatters perform synchronous I/O when reading to the request.
        /// This has poor performance characteristics with ASP.NET Core apps and may result
        /// in application deadlocks. This option forces ASP.NET Core to buffer requests preventing this scenario.
        /// </param>
        /// <returns>The application builder.</returns>
        public static IApplicationBuilder UseWebApi(this IApplicationBuilder builder, HttpServer httpServer, bool bufferRequests = true)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            if (httpServer == null)
            {
                throw new ArgumentNullException("httpServer");
            }

            HttpConfiguration configuration = httpServer.Configuration;
            Contract.Assert(configuration != null);

            HttpMessageHandlerOptions options = CreateOptions(builder, httpServer, configuration, bufferRequests);
            return UseMessageHandler(builder, options);
        }

        private static IApplicationBuilder UseMessageHandler(this IApplicationBuilder builder, HttpMessageHandlerOptions options)
        {
            Contract.Assert(builder != null);
            Contract.Assert(options != null);

            return builder.UseMiddleware<HttpMessageHandlerAdapter>(options);
        }

        private static HttpMessageHandlerOptions CreateOptions(IApplicationBuilder builder, HttpServer server,
            HttpConfiguration configuration, bool bufferRequests)
        {
            Contract.Assert(builder != null);
            Contract.Assert(server != null);
            Contract.Assert(configuration != null);

            ServicesContainer services = configuration.Services;
            Contract.Assert(services != null);

            IHostBufferPolicySelector bufferPolicySelector = services.GetHostBufferPolicySelector()
                ?? new AspNetCoreBufferPolicySelector(bufferRequests);
            IExceptionLogger exceptionLogger = ExceptionServices.GetLogger(services);
            IExceptionHandler exceptionHandler = ExceptionServices.GetHandler(services);

            return new HttpMessageHandlerOptions
            {
                MessageHandler = server,
                BufferPolicySelector = bufferPolicySelector,
                ExceptionLogger = exceptionLogger,
                ExceptionHandler = exceptionHandler,
            };
        }
    }
}
