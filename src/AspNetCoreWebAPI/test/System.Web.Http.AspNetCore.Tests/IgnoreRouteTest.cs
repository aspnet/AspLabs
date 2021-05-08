// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.AspNetCore.ExceptionHandling;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using Xunit;

namespace System.Web.Http.AspNetCore
{
    public class IgnoreRouteIntegrationTests
    {
        [Fact]
        public async Task Invoke_IfRouteIsIgnored_CallsNextMiddleware()
        {
            // Arrange
            var expectedStatusCode = 123;
            var pathToIgnoreRoute = "ignore";
            var next = CreateRequestDelegate(expectedStatusCode);

            using HttpServer server = new HttpServer();
            server.Configuration.Routes.IgnoreRoute("IgnoreRouteName", pathToIgnoreRoute);
            server.Configuration.MapHttpAttributeRoutes(); // See IgnoreController

            var product = CreateProductUnderTest(next, server);
            var context = GetStandardHttpContext(pathToIgnoreRoute);

            // Act
            await product.Invoke(context);

            // Assert
            Assert.Equal(expectedStatusCode, context.Response.StatusCode);
        }

        [Fact]
        public async Task Invoke_IfRouteIsIgnored_WithConstraints_CallsNextMiddleware()
        {
            // Arrange
            var expectedStatusCode = 456;
            var pathToIgnoreRoute = "constraint/10";
            var next = CreateRequestDelegate(expectedStatusCode);

            using HttpServer server = new HttpServer();
            server.Configuration.Routes.IgnoreRoute("Constraints", "constraint/{id}", constraints: new { constraint = new CustomConstraint() });
            server.Configuration.MapHttpAttributeRoutes(); // See IgnoreController

            var product = CreateProductUnderTest(next, server);
            var context = GetStandardHttpContext(pathToIgnoreRoute);

            // Act
            await product.Invoke(context);

            // Assert
            Assert.Equal(expectedStatusCode, context.Response.StatusCode);
        }

        private static RequestDelegate CreateRequestDelegate(int statusCode)
        {
            return context =>
            {
                context.Response.StatusCode = statusCode;
                return Task.CompletedTask;
            };
        }

        private static HttpMessageHandlerAdapter CreateProductUnderTest(RequestDelegate next, HttpMessageHandler messageHandler)
        {
            return new HttpMessageHandlerAdapter(next: next, options: new HttpMessageHandlerOptions
            {
                MessageHandler = messageHandler,
                BufferPolicySelector = new Mock<IHostBufferPolicySelector>().Object,
                ExceptionLogger = new EmptyExceptionLogger(),
                ExceptionHandler = new Mock<IExceptionHandler>().Object
            },
            Mock.Of<IHostApplicationLifetime>());
        }

        public class IgnoreController : ApiController
        {
            [Route("ignore")]
            [Route("constraint/10")]
            public IHttpActionResult Get()
            {
                return Ok();
            }
        }

        private static DefaultHttpContext GetStandardHttpContext(string pathToIgnoreRoute)
        {
            var httpContext = new DefaultHttpContext
            {
                Request =
                {
                    Method = "GET",
                    Scheme = "http",
                    Host = new HostString("somehost"),
                    PathBase = "/vroot",
                    Path = $"/{pathToIgnoreRoute}/",
                },
            };

            var hostingEnvironment = Mock.Of<IHostEnvironment>(m => m.EnvironmentName == "Development");
            var services = Mock.Of<IServiceProvider>(m => m.GetService(typeof(IHostEnvironment)) == hostingEnvironment);
            httpContext.RequestServices = services;

            return httpContext;
        }

        public class CustomConstraint : IHttpRouteConstraint
        {
            public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName,
                IDictionary<string, object> values, HttpRouteDirection routeDirection)
            {
                long id;
                if (values.ContainsKey("id")
                    && long.TryParse(values["id"].ToString(), out id)
                    && (id == 10))
                {
                    return true;
                }

                return false;
            }
        }
    }
}
