// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;
using Xunit;

namespace System.Web.Http.AspNetCore.ExceptionHandling
{
    public class DefaultExceptionHandlerTests
    {
        [Fact]
        public async Task HandleAsync_HandlesExceptionViaCreateErrorResponse()
        {
            IExceptionHandler product = CreateProductUnderTest();

            // Arrange
            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                expectedRequest.SetRequestContext(new HttpRequestContext { IncludeErrorDetail = true });
                ExceptionHandlerContext context = CreateValidContext(expectedRequest);
                CancellationToken cancellationToken = CancellationToken.None;

                // Act
                await product.HandleAsync(context, cancellationToken);

                // Assert
                IHttpActionResult result = context.Result;
                ResponseMessageResult typedResult = Assert.IsType<ResponseMessageResult>(result);
                using (HttpResponseMessage response = typedResult.Response)
                {
                    Assert.NotNull(response);

                    using (HttpResponseMessage expectedResponse = expectedRequest.CreateErrorResponse(
                        HttpStatusCode.InternalServerError, context.ExceptionContext.Exception))
                    {
                        AssertErrorResponse(expectedResponse, response);
                    }

                    Assert.Same(expectedRequest, response.RequestMessage);
                }
            }
        }

        private static void AssertErrorResponse(HttpResponseMessage expected, HttpResponseMessage actual)
        {
            Assert.NotNull(expected); // Guard
            ObjectContent<HttpError> expectedContent = Assert.IsType<ObjectContent<HttpError>>(expected.Content); // Guard
            Assert.NotNull(expectedContent.Formatter); // Guard

            Assert.NotNull(actual);
            Assert.Equal(expected.StatusCode, actual.StatusCode);
            ObjectContent<HttpError> actualContent = Assert.IsType<ObjectContent<HttpError>>(actual.Content);
            Assert.NotNull(actualContent.Formatter);
            Assert.Same(expectedContent.Formatter.GetType(), actualContent.Formatter.GetType());
            Assert.Equal(expectedContent.Value, actualContent.Value);
            Assert.Same(expected.RequestMessage, actual.RequestMessage);
        }

        private static ExceptionHandlerContext CreateContext(ExceptionContext exceptionContext)
        {
            return new ExceptionHandlerContext(exceptionContext);
        }

        private static Exception CreateException()
        {
            return new NotSupportedException();
        }

        private static DefaultExceptionHandler CreateProductUnderTest()
        {
            return new DefaultExceptionHandler();
        }

        private static HttpRequestMessage CreateRequest()
        {
            return new HttpRequestMessage();
        }

        private static ExceptionHandlerContext CreateValidContext(HttpRequestMessage request)
        {
            return CreateContext(new ExceptionContext(CreateException(),
                                                      ExceptionCatchBlocks.HttpServer,
                                                      request));
        }
    }
}
