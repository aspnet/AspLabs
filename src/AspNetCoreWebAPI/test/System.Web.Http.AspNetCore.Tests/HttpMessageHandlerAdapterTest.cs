// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Hosting;
using System.Web.Http.Results;
using System.Web.Http.Routing;
using Xunit;

namespace System.Web.Http.AspNetCore
{
    public class HttpMessageHandlerAdapterTest
    {
        [Fact]
        public async Task Invoke_BuildsAppropriateRequestMessage()
        {
            var handler = CreateOKHandlerStub();
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);

            await adapter.Invoke(new DefaultHttpContext
            {
                Request =
                {
                    Method = "GET",
                    Scheme = "http",
                    Host = new HostString("localhost", 80),
                    PathBase = "/vroot",
                    Path = "/api/customers",
                },
            });

            var request = handler.Request;
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("http://localhost/vroot/api/customers", request.RequestUri.AbsoluteUri);
        }

        [Fact]
        public async Task Invoke_BuildsUriWithQueryStringIfPresent()
        {
            var handler = CreateOKHandlerStub();
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var adapter = CreateProductUnderTest(new HttpMessageHandlerOptions
            {
                MessageHandler = handler,
                BufferPolicySelector = bufferPolicySelector,
            });

            await adapter.Invoke(new DefaultHttpContext
            {
                Request =
                {
                    Method = "GET",
                    Scheme = "http",
                    Host = new HostString("localhost", 80),
                    PathBase = "/vroot",
                    Path = "/api/customers",
                    QueryString = new QueryString("?id=45"),
                },
            });

            var request = handler.Request;
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("http://localhost/vroot/api/customers?id=45", request.RequestUri.AbsoluteUri);
        }

        [Fact]
        public async Task Invoke_BuildsUriWithHostAndPort()
        {
            var handler = CreateOKHandlerStub();
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);

            await adapter.Invoke(new DefaultHttpContext
            {
                Request =
                {
                    Method = "GET",
                    Scheme = "http",
                    Host = new HostString("localhost", 12345),
                    PathBase = "/vroot",
                    Path = "/api/customers",
                },
            });

            var request = handler.Request;
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("http://localhost:12345/vroot/api/customers", request.RequestUri.AbsoluteUri);
        }

        [Theory]
        [InlineData("a b")]
        // reserved characters
        [InlineData("!*'();:@&=$,[]")]
        // common unreserved characters
        [InlineData(@"-_.~+""<>^`{|}")]
        // random unicode characters
        [InlineData("激光這")]
        [InlineData("?#")]
        public async Task Invoke_CreatesUri_ThatGeneratesCorrectlyDecodedStrings(string decodedId)
        {
            var handler = CreateOKHandlerStub();
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);
            var route = new HttpRoute("api/customers/{id}");

            await adapter.Invoke(new DefaultHttpContext
            {
                Request =
                {
                    Method = "GET",
                    Scheme = "http",
                    Host = new HostString("localhost", 80),
                    PathBase = "/vroot",
                    Path = "/api/customers/" + decodedId,
                },
            });
            IHttpRouteData routeData = route.GetRouteData("/vroot", handler.Request);

            Assert.NotNull(routeData);
            Assert.Equal(decodedId, routeData.Values["id"]);
        }

        [Fact]
        public async Task Invoke_AddsRequestHeadersToRequestMessage()
        {
            var handler = CreateOKHandlerStub();
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);

            await adapter.Invoke(new DefaultHttpContext
            {
                Request =
                {
                    Method = "GET",
                    Scheme = "http",
                    Host = new HostString("localhost"),
                    PathBase = "/vroot",
                    Path = "/api/customers/",
                    ContentLength = 45,
                    Headers =
                    {
                        ["Accept"] = new StringValues(new[] { "application/json", "application/xml" }),
                    }
                },
            });

            var request = handler.Request;
            Assert.Equal(2, request.Headers.Count());
            Assert.Equal(new string[] { "application/json", "application/xml" }, request.Headers.Accept.Select(mediaType => mediaType.ToString()).ToArray());
            Assert.Equal("localhost", request.Headers.Host);
            Assert.Single(request.Content.Headers);
            Assert.Equal(45, request.Content.Headers.ContentLength);
        }

        [Fact]
        public async Task Invoke_SetsRequestBodyOnRequestMessage()
        {
            string body = null;
            Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync = async (r, i) =>
            {
                body = await r.Content.ReadAsStringAsync();
                return new HttpResponseMessage();
            };
            var handler = CreateLambdaMessageHandler(sendAsync);
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var expectedBody = "This is the request body.";
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);

            await adapter.Invoke(new DefaultHttpContext
            {
                Request =
                {
                    Method = "GET",
                    Scheme = "http",
                    Host = new HostString("localhost", 80),
                    PathBase = "/vroot",
                    Path = "/api/customers/",
                    Body = new MemoryStream(Encoding.UTF8.GetBytes(expectedBody)),
                },
            });

            Assert.Equal(expectedBody, body);
        }

        [Fact]
        public async Task Invoke_IfBufferPolicyEnablesInputBuffering_BuffersAndDrainsRequest()
        {
            // Arrange
            IHostBufferPolicySelector bufferPolicySelector = CreateBufferPolicySelector(bufferInput: true,
                bufferOutput: false);

            Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync = async (r, i) =>
            {
                await Task.Yield();
                return new HttpResponseMessage();
            };

            using HttpMessageHandler messageHandler = CreateLambdaMessageHandler(sendAsync);
            using HttpMessageHandlerAdapter product = CreateProductUnderTest(CreateValidOptions(messageHandler,
                bufferPolicySelector));

            var nonSeekableStream = new Mock<MemoryStream>(Encoding.UTF8.GetBytes("Hello world")) { CallBase = true };
            nonSeekableStream.Setup(s => s.CanSeek).Returns(false);
            var httpContext = new DefaultHttpContext
            {
                Request =
                {
                    Method = "GET",
                    Scheme = "http",
                    Host = new HostString("localhost", 80),
                    PathBase = "/vroot",
                    Path = "/api/customers/",
                    Body = nonSeekableStream.Object,
                },
            };

            // Act & Assert
            await product.Invoke(httpContext);

            Assert.NotSame(nonSeekableStream.Object, httpContext.Request.Body);
            Assert.True(httpContext.Request.Body.CanSeek);
        }

        [Fact]
        public async Task Invoke_SetsClientCertificate()
        {
            var handler = CreateOKHandlerStub();
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var httpContext = GetStandardHttpContext();
            var clientCert = Mock.Of<X509Certificate2>();
            var connectionFeature = Mock.Of<ITlsConnectionFeature>(f => f.ClientCertificate == clientCert);
            httpContext.Features.Set(connectionFeature);

            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);

            await adapter.Invoke(httpContext);

            var request = handler.Request;
            Assert.Same(clientCert, request.GetClientCertificate());
        }

        [Fact]
        public async Task Invoke_CallsMessageHandler_WithEnvironmentCancellationToken()
        {
            var handler = CreateOKHandlerStub();
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var httpContext = GetStandardHttpContext();
            var cancellationToken = new CancellationToken();
            httpContext.RequestAborted = cancellationToken;
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);

            await adapter.Invoke(httpContext);

            Assert.Equal(cancellationToken, handler.CancellationToken);
        }

        [Fact]
        public async Task Invoke_CallsMessageHandler_WithEnvironmentUser()
        {
            var handler = CreateOKHandlerStub();
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var user = new ClaimsPrincipal();
            var httpContext = GetStandardHttpContext();
            httpContext.User = user;
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);

            await adapter.Invoke(httpContext);

            Assert.Equal(user, handler.User);
        }

        [Fact]
        public async Task Invoke_Throws_IfMessageHandlerReturnsNull()
        {
            HttpResponseMessage response = null;
            var handler = new HandlerStub() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var httpContext = GetStandardHttpContext();
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => adapter.Invoke(httpContext));

            Assert.Equal("The message handler did not return a response message.", ex.Message);
        }

        [Fact]
        public async Task Invoke_DoesNotCallNext_IfMessageHandlerDoesNotReturn404()
        {
            RequestDelegate next = _ => throw new NotSupportedException("This should not be called.");
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            var handler = new HandlerStub() { Response = response, AddNoRouteMatchedKey = true };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var httpContext = GetStandardHttpContext();
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options, next);

            // Does not throw.
            await adapter.Invoke(httpContext);
        }

        [Fact]
        public async Task Invoke_DoesNotCallNext_IfMessageHandlerDoesNotAddNoRouteMatchedProperty()
        {
            RequestDelegate next = _ => throw new NotSupportedException("This should not be called.");
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NotFound);
            var handler = new HandlerStub() { Response = response, AddNoRouteMatchedKey = false };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var httpContext = GetStandardHttpContext();
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options, next);

            // Does not throw.
            await adapter.Invoke(httpContext);
        }

        [Fact]
        public async Task Invoke_CallsNext_IfMessageHandlerReturns404WithNoRouteMatched()
        {
            var invoked = false;
            RequestDelegate next = (next) => { invoked = true; return Task.CompletedTask; };
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NotFound);
            var handler = new HandlerStub() { Response = response, AddNoRouteMatchedKey = true };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var httpContext = GetStandardHttpContext();
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options, next);

            await adapter.Invoke(httpContext);

            Assert.True(invoked);
        }

        [Fact]
        public async Task Invoke_SetsResponseStatusCode()
        {
            var response = new HttpResponseMessage(HttpStatusCode.InsufficientStorage);
            var handler = new HandlerStub() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var httpContext = GetStandardHttpContext();
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);

            await adapter.Invoke(httpContext);

            Assert.Equal(507, httpContext.Response.StatusCode);
        }

        [Fact]
        public async Task Invoke_SetsResponseHeaders()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Headers.Location = new Uri("http://www.location.com/");
            response.Content = new StringContent(@"{""x"":""y""}", Encoding.UTF8, "application/json");
            var handler = new HandlerStub() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var httpContext = GetStandardHttpContext();
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);

            await adapter.Invoke(httpContext);

            var responseHeaders = httpContext.Response.Headers;
            Assert.Equal(3, responseHeaders.Count);
            Assert.Equal("http://www.location.com/", Assert.Single(responseHeaders["Location"]));
            Assert.Equal("9", Assert.Single(responseHeaders["Content-Length"]));
            Assert.Equal("application/json; charset=utf-8", Assert.Single(responseHeaders["Content-Type"]));
        }

        [Fact]
        public async Task Invoke_SetsResponseBody()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var expectedBody = @"{""x"":""y""}";
            response.Content = new StringContent(expectedBody, Encoding.UTF8, "application/json");
            var handler = new HandlerStub() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var httpContext = GetStandardHttpContext();
            var responseStream = new MemoryStream();
            httpContext.Response.Body = responseStream;
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);

            await adapter.Invoke(httpContext);

            responseStream.Seek(0, SeekOrigin.Begin);
            byte[] bodyBytes = new byte[9];
            int charsRead = responseStream.Read(bodyBytes, 0, 9);
            // Assert that we can read 9 characters and no more characters after that
            Assert.Equal(9, charsRead);
            Assert.Equal(-1, responseStream.ReadByte());
            Assert.Equal(expectedBody, Encoding.UTF8.GetString(bodyBytes));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Invoke_RespectsOutputBufferingSetting(bool bufferOutput)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new ObjectContent<string>("blue", new JsonMediaTypeFormatter());
            var handler = new HandlerStub() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: bufferOutput);
            var httpContext = GetStandardHttpContext();
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);

            await adapter.Invoke(httpContext);

            if (bufferOutput)
            {
                Assert.NotNull(httpContext.Response.ContentLength);
            }
            else
            {
                Assert.Null(httpContext.Response.ContentLength);
            }
        }

        [Fact]
        public async Task Invoke_AddsZeroContentLengthHeader_WhenThereIsNoContent()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var handler = new HandlerStub() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var httpContext = GetStandardHttpContext();
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);

            await adapter.Invoke(httpContext);

            var responseHeaders = httpContext.Response.Headers;
            Assert.Equal("0", responseHeaders["Content-Length"][0]);
        }

        [Fact]
        public async Task Invoke_IfTransferEncodingChunkedAndContentLengthAreBothSet_IgnoresContentLength()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("Hello world")));
            response.Headers.TransferEncodingChunked = true;
            var handler = new HandlerStub() { Response = response };
            var bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            var httpContext = GetStandardHttpContext();
            var options = CreateValidOptions(handler, bufferPolicySelector);
            var adapter = CreateProductUnderTest(options);

            await adapter.Invoke(httpContext);

            var responseHeaders = httpContext.Response.Headers;
            Assert.False(responseHeaders.ContainsKey("Content-Length"));
        }

        [Fact]
        public async Task Invoke_IfTransferEncodingIsJustChunked_DoesNotCopyHeader()
        {
            // Arrange
            IHostBufferPolicySelector bufferPolicy = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);

            using HttpResponseMessage response = CreateResponse();
            using HttpMessageHandler messageHandler = CreateStubMessageHandler(response);
            using HttpMessageHandlerAdapter product = CreateProductUnderTest(CreateValidOptions(messageHandler,
                bufferPolicy));
            response.Headers.TransferEncodingChunked = true;

            var httpContext = GetStandardHttpContext();

            // Act
            await product.Invoke(httpContext);

            // Assert
            var responseHeaders = httpContext.Response.Headers;
            Assert.DoesNotContain("Transfer-Encoding", responseHeaders.Keys);
        }

        [Fact]
        public async Task Invoke_IfTransferEncodingIsIdentity_DoesCopyHeader()
        {
            // Arrange
            IHostBufferPolicySelector bufferPolicy = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);

            using HttpResponseMessage response = CreateResponse();
            using HttpMessageHandler messageHandler = CreateStubMessageHandler(response);
            using HttpMessageHandlerAdapter product = CreateProductUnderTest(CreateValidOptions(messageHandler,
                bufferPolicy));
            response.Headers.TransferEncoding.Add(new TransferCodingHeaderValue("identity"));
            var httpContext = GetStandardHttpContext();

            // Act
            await product.Invoke(httpContext);

            // Assert
            var responseHeaders = httpContext.Response.Headers;
            Assert.Contains("Transfer-Encoding", responseHeaders.Keys);
            Assert.Equal("identity", responseHeaders["Transfer-Encoding"]);
        }

        [Fact]
        public async Task Invoke_IfTransferEncodingIsIdentityChunked_DoesCopyHeader()
        {
            // Arrange
            IHostBufferPolicySelector bufferPolicy = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);

            using HttpResponseMessage response = CreateResponse();
            using HttpMessageHandler messageHandler = CreateStubMessageHandler(response);
            using HttpMessageHandlerAdapter product = CreateProductUnderTest(CreateValidOptions(messageHandler,
                bufferPolicy));
            response.Headers.TransferEncoding.Add(new TransferCodingHeaderValue("identity"));
            response.Headers.TransferEncodingChunked = true;
            Assert.Equal("identity, chunked", response.Headers.TransferEncoding.ToString()); // Guard
            var httpContext = GetStandardHttpContext();

            // Act
            await product.Invoke(httpContext);

            // Assert
            var responseHeaders = httpContext.Response.Headers;
            Assert.Contains("Transfer-Encoding", responseHeaders.Keys);
            Assert.Equal("identity,chunked", responseHeaders["Transfer-Encoding"]);
        }

        [Fact]
        public async Task Invoke_IfBufferingFaults_DisposesOriginalResponse()
        {
            // Arrange
            IHostBufferPolicySelector bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false,
                bufferOutput: true);

            using HttpContent content = CreateFaultingContent();
            using SpyDisposeHttpResponseMessage spy = new SpyDisposeHttpResponseMessage(content);
            using HttpMessageHandler messageHandler = CreateStubMessageHandler(spy);
            HttpMessageHandlerOptions options = CreateValidOptions(messageHandler);
            options.BufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: true);

            using HttpMessageHandlerAdapter product = CreateProductUnderTest(options);
            using MemoryStream output = new MemoryStream();
            var httpContext = GetStandardHttpContext();
            var hostingEnvironment = Mock.Of<IHostEnvironment>(m => m.EnvironmentName == "Development");
            var services = Mock.Of<IServiceProvider>(m => m.GetService(typeof(IHostEnvironment)) == hostingEnvironment);
            httpContext.RequestServices = services;

            // Act
            await product.Invoke(httpContext);

            // Assert
            Assert.True(spy.Disposed);
        }

        [Fact]
        public async Task Invoke_IfBufferingFaults_CallsExceptionServices()
        {
            // Arrange
            Exception expectedException = CreateException();

            using HttpContent content = CreateFaultingContent(expectedException);
            using HttpResponseMessage expectedResponse = CreateResponse(content);
            using HttpMessageHandler messageHandler = CreateStubMessageHandler(expectedResponse);
            Mock<IExceptionLogger> exceptionLoggerMock = CreateStubExceptionLoggerMock();
            IExceptionLogger exceptionLogger = exceptionLoggerMock.Object;

            Mock<IExceptionHandler> exceptionHandlerMock = CreateStubExceptionHandlerMock();
            IExceptionHandler exceptionHandler = exceptionHandlerMock.Object;

            HttpMessageHandlerOptions options = CreateValidOptions(messageHandler);
            options.BufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: true);
            options.ExceptionLogger = exceptionLogger;
            options.ExceptionHandler = exceptionHandler;

            using HttpMessageHandlerAdapter product = CreateProductUnderTest(options);
            using CancellationTokenSource tokenSource = CreateCancellationTokenSource();
            CancellationToken expectedCancellationToken = tokenSource.Token;
            var httpContext = GetStandardHttpContext();
            httpContext.RequestAborted = expectedCancellationToken;

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => product.Invoke(httpContext));

            Func<ExceptionContext, bool> exceptionContextMatches = (c) =>
                c != null
                && c.Exception == expectedException
                && c.CatchBlock == AspNetCoreExceptionCatchBlocks.HttpMessageHandlerAdapterBufferContent
                && c.Request != null
                && c.Response == expectedResponse;

            exceptionLoggerMock.Verify(l => l.LogAsync(
                It.Is<ExceptionLoggerContext>(c => exceptionContextMatches(c.ExceptionContext)),
                expectedCancellationToken), Times.Once());
            exceptionHandlerMock.Verify(h => h.HandleAsync(
                It.Is<ExceptionHandlerContext>((c) => exceptionContextMatches(c.ExceptionContext)),
                expectedCancellationToken), Times.Once());
        }

        [Fact]
        public async Task Invoke_IfBufferingCancels_DoesNotCallExceptionServices()
        {
            // Arrange
            Exception expectedException = new OperationCanceledException();

            using HttpContent content = CreateFaultingContent(expectedException);
            using HttpResponseMessage expectedResponse = CreateResponse(content);
            using HttpMessageHandler messageHandler = CreateStubMessageHandler(expectedResponse);
            Mock<IExceptionLogger> exceptionLoggerMock = new Mock<IExceptionLogger>(MockBehavior.Strict);
            IExceptionLogger exceptionLogger = exceptionLoggerMock.Object;

            Mock<IExceptionHandler> exceptionHandlerMock = new Mock<IExceptionHandler>(MockBehavior.Strict);
            IExceptionHandler exceptionHandler = exceptionHandlerMock.Object;

            HttpMessageHandlerOptions options = CreateValidOptions(messageHandler);
            options.BufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: true);
            options.ExceptionLogger = exceptionLogger;
            options.ExceptionHandler = exceptionHandler;

            using HttpMessageHandlerAdapter product = CreateProductUnderTest(options);
            using CancellationTokenSource tokenSource = CreateCancellationTokenSource();
            var httpContext = GetStandardHttpContext();
            httpContext.RequestAborted = tokenSource.Token;

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() => product.Invoke(httpContext));
        }

        [Fact]
        public async Task Invoke_IfExceptionHandlerSetsNullResult_PropogatesFaultedTaskException()
        {
            // Arrange
            Exception expectedException = CreateExceptionWithCallStack();
            string expectedStackTrace = expectedException.StackTrace;

            using HttpContent content = CreateFaultingContent(expectedException);
            using HttpResponseMessage response = CreateResponse(content);
            using HttpMessageHandler messageHandler = CreateStubMessageHandler(response);
            IExceptionLogger exceptionLogger = CreateStubExceptionLogger();
            IExceptionHandler exceptionHandler = CreateExceptionHandler(result: null);

            HttpMessageHandlerOptions options = CreateValidOptions(messageHandler);
            options.BufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: true);
            options.ExceptionLogger = exceptionLogger;
            options.ExceptionHandler = exceptionHandler;

            using HttpMessageHandlerAdapter product = CreateProductUnderTest(options);
            var httpContext = GetStandardHttpContext();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => product.Invoke(httpContext));
            Assert.Same(expectedException, exception);
            Assert.NotNull(exception.StackTrace);
            Assert.StartsWith(expectedStackTrace, exception.StackTrace);
        }

        [Fact]
        public async Task Invoke_IfExceptionHandlerHandlesException_SendsResponse()
        {
            // Arrange
            Exception expectedException = CreateException();
            string expectedErrorContents = "Sorry";
            HttpStatusCode expectedErrorStatusCode = HttpStatusCode.BadRequest;

            using HttpContent content = CreateFaultingContent();
            using HttpResponseMessage response = CreateResponse(content);
            using HttpMessageHandler messageHandler = CreateStubMessageHandler(response);
            using StringContent errorContent = new StringContent(expectedErrorContents);
            using HttpResponseMessage errorResponse = CreateResponse(errorContent);
            errorResponse.StatusCode = expectedErrorStatusCode;
            errorResponse.Content = errorContent;

            HttpMessageHandlerOptions options = CreateValidOptions(messageHandler);
            options.BufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: true);
            options.ExceptionLogger = CreateStubExceptionLogger();
            options.ExceptionHandler = CreateExceptionHandler(new ResponseMessageResult(errorResponse));

            using HttpMessageHandlerAdapter product = CreateProductUnderTest(options);
            using MemoryStream output = new MemoryStream();
            var httpContext = GetStandardHttpContext();
            httpContext.Response.Body = output;

            // Act
            await product.Invoke(httpContext);

            // Assert
            Assert.Equal((int)expectedErrorStatusCode, httpContext.Response.StatusCode);
            using HttpRequestMessage request = CreateRequest(includeErrorDetail: true);
            Assert.Equal(expectedErrorContents, Encoding.UTF8.GetString(output.ToArray()));
        }

        [Fact]
        public async Task Invoke_IfBufferingFaultsAndUsingListConstructor_SendsErrorResponse()
        {
            // Arrange
            Exception expectedException = CreateException();
            IHostBufferPolicySelector bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false,
                bufferOutput: true);

            using HttpContent content = CreateFaultingContent(expectedException);
            using HttpResponseMessage response = CreateResponse(content);
            using HttpMessageHandler messageHandler = CreateStubMessageHandler(response);
            using HttpMessageHandlerAdapter product = CreateProductUnderTest(CreateValidOptions(messageHandler,
                bufferPolicySelector));
            using MemoryStream output = new MemoryStream();

            var httpContext = GetStandardHttpContext();
            httpContext.Response.Body = output;
            var hostingEnvironment = Mock.Of<IHostEnvironment>(m => m.EnvironmentName == "Development");
            var services = Mock.Of<IServiceProvider>(m => m.GetService(typeof(IHostEnvironment)) == hostingEnvironment);
            httpContext.RequestServices = services;

            // Act
            await product.Invoke(httpContext);

            // Assert
            Assert.Equal(500, httpContext.Response.StatusCode);
            using HttpRequestMessage request = CreateRequest(includeErrorDetail: true);
            using HttpResponseMessage expectedResponse = request.CreateErrorResponse(
                HttpStatusCode.InternalServerError, expectedException);
            string expectedContents = await expectedResponse.Content.ReadAsStringAsync();
            Assert.Contains("An error has occurred.", Encoding.UTF8.GetString(output.ToArray()));
        }

        [Fact]
        public async Task Invoke_IfBufferingErrorFaults_DisposesErrorResponse()
        {
            // Arrange
            Exception expectedOriginalException = CreateException();
            Exception expectedErrorException = CreateException();

            using HttpContent content = CreateFaultingContent(expectedOriginalException);
            using HttpResponseMessage expectedOriginalResponse = CreateResponse(content);
            using HttpMessageHandler messageHandler = CreateStubMessageHandler(expectedOriginalResponse);
            using HttpContent errorContent = CreateFaultingContent(expectedErrorException);
            using SpyDisposeHttpResponseMessage spy = new SpyDisposeHttpResponseMessage(errorContent);
            using CancellationTokenSource tokenSource = CreateCancellationTokenSource();
            HttpMessageHandlerOptions options = CreateValidOptions(messageHandler);
            options.BufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: true);
            options.ExceptionHandler = CreateExceptionHandler(new ResponseMessageResult(spy));

            using HttpMessageHandlerAdapter product = CreateProductUnderTest(options);
            CancellationToken expectedCancellationToken = tokenSource.Token;
            var httpContext = GetStandardHttpContext();

            // Act
            await product.Invoke(httpContext);

            // Assert
            Assert.True(spy.Disposed);
        }

        [Fact]
        public async Task Invoke_IfStreamingFaults_ReturnsCanceledTask()
        {
            // Arrange
            IHostBufferPolicySelector bufferPolicySelector = CreateBufferPolicySelector(bufferInput: false,
                bufferOutput: false);

            using HttpContent content = CreateFaultingContent();
            using HttpResponseMessage response = CreateResponse(content);
            using HttpMessageHandler messageHandler = CreateStubMessageHandler(response);
            using HttpMessageHandlerAdapter product = CreateProductUnderTest(CreateValidOptions(messageHandler,
                bufferPolicySelector));
            var httpContext = GetStandardHttpContext();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => product.Invoke(httpContext));
        }

        [Fact]
        public async Task Invoke_IfStreamingFaults_CallsExceptionLogger()
        {
            // Arrange
            Exception expectedException = CreateException();

            using HttpContent content = CreateFaultingContent(expectedException);
            using HttpResponseMessage expectedResponse = CreateResponse(content);
            using HttpMessageHandler messageHandler = CreateStubMessageHandler(expectedResponse);
            Mock<IExceptionLogger> mock = CreateStubExceptionLoggerMock();
            IExceptionLogger exceptionLogger = mock.Object;

            HttpMessageHandlerOptions options = CreateValidOptions(messageHandler);
            options.BufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            options.ExceptionLogger = exceptionLogger;

            using HttpMessageHandlerAdapter product = CreateProductUnderTest(options);
            using CancellationTokenSource tokenSource = CreateCancellationTokenSource();
            CancellationToken expectedCancellationToken = tokenSource.Token;
            var httpContext = GetStandardHttpContext();
            httpContext.RequestAborted = expectedCancellationToken;

            // Act & Assert
            await Assert.ThrowsAsync<TaskCanceledException>(() => product.Invoke(httpContext));

            mock.Verify(l => l.LogAsync(It.Is<ExceptionLoggerContext>((c) =>
                c != null
                && c.ExceptionContext != null
                && c.ExceptionContext.Exception == expectedException
                && c.ExceptionContext.CatchBlock ==
                    AspNetCoreExceptionCatchBlocks.HttpMessageHandlerAdapterStreamContent
                && c.ExceptionContext.Request != null
                && c.ExceptionContext.Response == expectedResponse),
                expectedCancellationToken), Times.Once());
        }

        [Fact]
        public async Task Invoke_IfStreamingCancels_DoesNotCallExceptionLogger()
        {
            // Arrange
            Exception expectedException = new OperationCanceledException();

            using HttpContent content = CreateFaultingContent(expectedException);
            using HttpResponseMessage expectedResponse = CreateResponse(content);
            using HttpMessageHandler messageHandler = CreateStubMessageHandler(expectedResponse);
            Mock<IExceptionLogger> mock = new Mock<IExceptionLogger>(MockBehavior.Strict);
            IExceptionLogger exceptionLogger = mock.Object;

            HttpMessageHandlerOptions options = CreateValidOptions(messageHandler);
            options.BufferPolicySelector = CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            options.ExceptionLogger = exceptionLogger;

            using HttpMessageHandlerAdapter product = CreateProductUnderTest(options);
            using CancellationTokenSource tokenSource = CreateCancellationTokenSource();
            CancellationToken expectedCancellationToken = tokenSource.Token;
            var httpContext = GetStandardHttpContext();

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(() => product.Invoke(httpContext));
        }

        [Fact]
        public async Task Invoke_IfTryComputeLengthThrows_CallsExceptionLogger()
        {
            // Arrange
            Exception expectedException = CreateException();

            using HttpContent content = CreateThrowingContent(expectedException);
            using HttpResponseMessage expectedResponse = CreateResponse(content);
            using HttpMessageHandler messageHandler = CreateStubMessageHandler(expectedResponse);
            Mock<IExceptionLogger> mock = CreateStubExceptionLoggerMock();
            IExceptionLogger exceptionLogger = mock.Object;

            HttpMessageHandlerOptions options = CreateValidOptions(messageHandler);
            options.ExceptionLogger = exceptionLogger;

            using HttpMessageHandlerAdapter product = CreateProductUnderTest(options);
            using CancellationTokenSource tokenSource = CreateCancellationTokenSource();
            CancellationToken expectedCancellationToken = tokenSource.Token;
            var httpContext = GetStandardHttpContext();
            httpContext.RequestAborted = expectedCancellationToken;

            // Act
            await product.Invoke(httpContext);

            // Assert
            mock.Verify(l => l.LogAsync(It.Is<ExceptionLoggerContext>((c) =>
                c != null
                && c.ExceptionContext != null
                && c.ExceptionContext.Exception == expectedException
                && c.ExceptionContext.CatchBlock ==
                    AspNetCoreExceptionCatchBlocks.HttpMessageHandlerAdapterComputeContentLength
                && c.ExceptionContext.Request != null
                && c.ExceptionContext.Response == expectedResponse),
                expectedCancellationToken), Times.Once());
        }

        [Fact]
        public async Task Invoke_IfTryComputeLengthThrows_SendsEmptyErrorResponse()
        {
            // Arrange
            Exception expectedException = CreateException();

            using HttpContent content = CreateThrowingContent(expectedException);
            using HttpResponseMessage response = CreateResponse(content);
            using HttpMessageHandler messageHandler = CreateStubMessageHandler(response);
            HttpMessageHandlerOptions options = CreateValidOptions(messageHandler);

            using HttpMessageHandlerAdapter product = CreateProductUnderTest(options);
            var httpContext = GetStandardHttpContext();

            // Act
            await product.Invoke(httpContext);

            // Assert
            Assert.Equal(500, httpContext.Response.StatusCode);
            Assert.Equal(0, httpContext.Response.ContentLength);
        }

        private static DefaultHttpContext GetStandardHttpContext()
        {
            return new DefaultHttpContext
            {
                Request =
                {
                    Method = "GET",
                    Scheme = "http",
                    Host = new HostString("localhost"),
                    PathBase = "/vroot",
                    Path = "/api/customers/",
                },
            };
        }

        private static IHostBufferPolicySelector CreateBufferPolicySelector(bool bufferInput, bool bufferOutput)
        {
            var mock = new Mock<IHostBufferPolicySelector>();
            mock.Setup(bps => bps.UseBufferedInputStream(It.IsAny<object>())).Returns(bufferInput);
            mock.Setup(bps => bps.UseBufferedOutputStream(It.IsAny<HttpResponseMessage>())).Returns(bufferOutput);
            return mock.Object;
        }

        private static CancellationTokenSource CreateCancellationTokenSource()
        {
            return new CancellationTokenSource();
        }

        private static IHostBufferPolicySelector CreateDummyBufferPolicy()
        {
            return new Mock<IHostBufferPolicySelector>(MockBehavior.Strict).Object;
        }

        private static HttpMessageHandler CreateDummyMessageHandler()
        {
            Mock<HttpMessageHandler> mock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mock.As<IDisposable>().Setup(c => c.Dispose());
            return mock.Object;
        }

        private static HttpMessageHandlerOptions CreateDummyOptions(HttpMessageHandler messageHandler)
        {
            return new HttpMessageHandlerOptions
            {
                MessageHandler = messageHandler,
                BufferPolicySelector = CreateDummyBufferPolicy(),
            };
        }

        private static Exception CreateException()
        {
            return new Exception();
        }

        private static Exception CreateExceptionWithCallStack()
        {
            try
            {
                throw CreateException();
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        private static Task CreateFaultedTask(Exception exception)
        {
            TaskCompletionSource<object> source = new TaskCompletionSource<object>();
            source.SetException(exception);
            return source.Task;
        }

        private static FaultingHttpContent CreateFaultingContent()
        {
            return CreateFaultingContent(CreateException());
        }

        private static FaultingHttpContent CreateFaultingContent(Exception exception)
        {
            return new FaultingHttpContent(exception);
        }

        private static HttpMessageHandler CreateLambdaMessageHandler(
            Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
        {
            return new LambdaHttpMessageHandler(sendAsync);
        }

        private static HandlerStub CreateOKHandlerStub()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            return new HandlerStub() { Response = response };
        }

        private static HttpMessageHandlerAdapter CreateProductUnderTest(HttpMessageHandlerOptions options, RequestDelegate next = null)
        {
            return new HttpMessageHandlerAdapter(next, options, Mock.Of<IHostApplicationLifetime>());
        }

        private static HttpRequestMessage CreateRequest()
        {
            return new HttpRequestMessage();
        }

        private static HttpRequestMessage CreateRequest(bool includeErrorDetail)
        {
            HttpRequestMessage request = CreateRequest();
            request.SetRequestContext(new HttpRequestContext
            {
                IncludeErrorDetail = includeErrorDetail
            });
            return request;
        }

        private static HttpResponseMessage CreateResponse()
        {
            return new HttpResponseMessage();
        }

        private static HttpResponseMessage CreateResponse(HttpContent content)
        {
            return new HttpResponseMessage
            {
                Content = content
            };
        }

        private static HttpMessageHandler CreateStubMessageHandler(HttpResponseMessage response)
        {
            return new LambdaHttpMessageHandler((r, c) => Task.FromResult(response));
        }

        private static ThrowingHttpContent CreateThrowingContent(Exception exception)
        {
            return new ThrowingHttpContent(exception);
        }

        private static HttpMessageHandlerOptions CreateValidOptions(HttpMessageHandler messageHandler)
        {
            IHostBufferPolicySelector bufferPolicy =
                CreateBufferPolicySelector(bufferInput: false, bufferOutput: false);
            return CreateValidOptions(messageHandler, bufferPolicy);
        }

        private static HttpMessageHandlerOptions CreateValidOptions(HttpMessageHandler messageHandler,
            IHostBufferPolicySelector bufferPolicySelector)
        {
            return new HttpMessageHandlerOptions
            {
                MessageHandler = messageHandler,
                BufferPolicySelector = bufferPolicySelector,
            };
        }

        private static Mock<IExceptionHandler> CreateStubExceptionHandlerMock()
        {
            Mock<IExceptionHandler> mock = new Mock<IExceptionHandler>(MockBehavior.Strict);
            mock
                .Setup(h => h.HandleAsync(It.IsAny<ExceptionHandlerContext>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0));
            return mock;
        }

        private static IExceptionLogger CreateStubExceptionLogger()
        {
            return CreateStubExceptionLoggerMock().Object;
        }

        private static Mock<IExceptionLogger> CreateStubExceptionLoggerMock()
        {
            Mock<IExceptionLogger> mock = new Mock<IExceptionLogger>(MockBehavior.Strict);
            mock
                .Setup(l => l.LogAsync(It.IsAny<ExceptionLoggerContext>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0));
            return mock;
        }

        public class HandlerStub : HttpMessageHandler
        {
            public HttpRequestMessage Request { get; private set; }
            public CancellationToken CancellationToken { get; private set; }
            public HttpResponseMessage Response { get; set; }
            public IPrincipal User { get; set; }
            public bool AddNoRouteMatchedKey { get; set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Request = request;
                CancellationToken = cancellationToken;
                User = Thread.CurrentPrincipal;

                if (AddNoRouteMatchedKey)
                {
                    request.Properties[HttpPropertyKeys.NoRouteMatched] = true;
                }

                return Task.FromResult<HttpResponseMessage>(Response);
            }
        }

        private class LambdaHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;

            public LambdaHttpMessageHandler(
                Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
            {
                _sendAsync = sendAsync;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                return _sendAsync.Invoke(request, cancellationToken);
            }
        }

        private class FaultingHttpContent : HttpContent
        {
            private readonly Exception _exception;

            public FaultingHttpContent(Exception exception)
            {
                _exception = exception;
            }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                return CreateFaultedTask(_exception);
            }

            protected override bool TryComputeLength(out long length)
            {
                length = 0;
                return false;
            }
        }

        private class SpyDisposeFaultingHttpContent : HttpContent
        {
            private readonly Exception _exception;

            public SpyDisposeFaultingHttpContent(Exception exception)
            {
                _exception = exception;
            }

            public bool Disposed { get; private set; }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                return CreateFaultedTask(_exception);
            }

            protected override bool TryComputeLength(out long length)
            {
                length = 0;
                return false;
            }

            protected override void Dispose(bool disposing)
            {
                Disposed = true;
                base.Dispose(disposing);
            }
        }

        private class SpyDisposeHttpMessageHandler : HttpMessageHandler
        {
            public bool Disposed { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            protected override void Dispose(bool disposing)
            {
                Disposed = true;
            }
        }

        private class SpyDisposeHttpResponseMessage : HttpResponseMessage
        {
            public SpyDisposeHttpResponseMessage(HttpContent content)
            {
                Content = content;
            }

            public bool Disposed { get; private set; }

            protected override void Dispose(bool disposing)
            {
                Disposed = true;
                base.Dispose(disposing);
            }
        }

        private class SpyDisposeStream : Stream
        {
            public bool Disposed { get; private set; }

            public override bool CanRead
            {
                get { return false; }
            }

            public override bool CanSeek
            {
                get { return false; }
            }

            public override bool CanWrite
            {
                get { return true; }
            }

            public override void Flush()
            {
            }

            public override long Length
            {
                get { return 0; }
            }

            public override long Position
            {
                get
                {
                    return 0;
                }
                set
                {
                }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return 0;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
            }

            protected override void Dispose(bool disposing)
            {
                Disposed = true;
                base.Dispose(disposing);
            }
        }

        private class ThrowingHttpContent : HttpContent
        {
            private readonly Exception _exception;

            public ThrowingHttpContent(Exception exception)
            {
                _exception = exception;
            }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                throw _exception;
            }

            protected override bool TryComputeLength(out long length)
            {
                throw _exception;
            }
        }

        private static IExceptionHandler CreateExceptionHandler(IHttpActionResult result)
        {
            Mock<IExceptionHandler> mock = new Mock<IExceptionHandler>(MockBehavior.Strict);
            mock
                .Setup(h => h.HandleAsync(It.IsAny<ExceptionHandlerContext>(), It.IsAny<CancellationToken>()))
                .Callback<ExceptionHandlerContext, CancellationToken>((c, i) => c.Result = result)
                .Returns(Task.CompletedTask);
            return mock.Object;
        }

    }
}
