// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Grpc.AspNetCore.Server;
using Grpc.AspNetCore.Server.Model;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Shared.HttpApi;
using Grpc.Shared.Server;
using Grpc.Tests.Shared;
using HttpApi;
using Microsoft.AspNetCore.Grpc.HttpApi.Internal.CallHandlers;
using Microsoft.AspNetCore.Grpc.HttpApi.Internal.Json;
using Microsoft.AspNetCore.Grpc.HttpApi.Tests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Primitives;
using Xunit;
using Xunit.Abstractions;
using MethodOptions = Grpc.Shared.Server.MethodOptions;
using Type = System.Type;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Tests
{
    public class UnaryServerCallHandlerTests : LoggedTest
    {
        public UnaryServerCallHandlerTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task HandleCallAsync_MatchingRouteValue_SetOnRequestMessage()
        {
            // Arrange
            HelloRequest? request = null;
            UnaryServerMethod<HttpApiGreeterService, HelloRequest, HelloReply> invoker = (s, r, c) =>
            {
                request = r;
                return Task.FromResult(new HelloReply { Message = $"Hello {r.Name}" });
            };

            var routeParameterDescriptors = new Dictionary<string, List<FieldDescriptor>>
            {
                ["name"] = new List<FieldDescriptor>(new[] { HelloRequest.Descriptor.FindFieldByNumber(HelloRequest.NameFieldNumber) }),
                ["sub.subfield"] = new List<FieldDescriptor>(new[]
                {
                    HelloRequest.Descriptor.FindFieldByNumber(HelloRequest.SubFieldNumber),
                    HelloRequest.Types.SubMessage.Descriptor.FindFieldByNumber(HelloRequest.Types.SubMessage.SubfieldFieldNumber)
                })
            };
            var descriptorInfo = TestHelpers.CreateDescriptorInfo(routeParameterDescriptors: routeParameterDescriptors);
            var unaryServerCallHandler = CreateCallHandler(invoker, descriptorInfo: descriptorInfo);
            var httpContext = TestHelpers.CreateHttpContext();
            httpContext.Request.RouteValues["name"] = "TestName!";
            httpContext.Request.RouteValues["sub.subfield"] = "Subfield!";

            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            Assert.NotNull(request);
            Assert.Equal("TestName!", request!.Name);
            Assert.Equal("Subfield!", request!.Sub.Subfield);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var responseJson = JsonDocument.Parse(httpContext.Response.Body);
            Assert.Equal("Hello TestName!", responseJson.RootElement.GetProperty("message").GetString());
        }

        [Theory]
        [InlineData("TestName!")]
        [InlineData("")]
        public async Task HandleCallAsync_ResponseBodySet_ResponseReturned(string name)
        {
            // Arrange
            HelloRequest? request = null;
            UnaryServerMethod<HttpApiGreeterService, HelloRequest, HelloReply> invoker = (s, r, c) =>
            {
                request = r;
                return Task.FromResult(new HelloReply { Message = r.Name });
            };

            var routeParameterDescriptors = new Dictionary<string, List<FieldDescriptor>>
            {
                ["name"] = new List<FieldDescriptor>(new[] { HelloRequest.Descriptor.FindFieldByNumber(HelloRequest.NameFieldNumber) })
            };
            var descriptorInfo = TestHelpers.CreateDescriptorInfo(
                responseBodyDescriptor: HelloReply.Descriptor.FindFieldByNumber(HelloReply.MessageFieldNumber),
                routeParameterDescriptors: routeParameterDescriptors);
            var unaryServerCallHandler = CreateCallHandler(
                invoker,
                descriptorInfo);
            var httpContext = TestHelpers.CreateHttpContext();
            httpContext.Request.RouteValues["name"] = name;

            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            Assert.NotNull(request);
            Assert.Equal(name, request!.Name);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var responseJson = JsonDocument.Parse(httpContext.Response.Body);
            Assert.Equal(name, responseJson.RootElement.GetString());
        }

        [Fact]
        public async Task HandleCallAsync_NullProperty_ResponseReturned()
        {
            // Arrange
            UnaryServerMethod<HttpApiGreeterService, HelloRequest, HelloReply> invoker = (s, r, c) =>
            {
                return Task.FromResult(new HelloReply { NullableMessage = null });
            };

            var routeParameterDescriptors = new Dictionary<string, List<FieldDescriptor>>
            {
                ["name"] = new List<FieldDescriptor>(new[] { HelloRequest.Descriptor.FindFieldByNumber(HelloRequest.NameFieldNumber) })
            };
            var descriptorInfo = TestHelpers.CreateDescriptorInfo(
                responseBodyDescriptor: HelloReply.Descriptor.FindFieldByNumber(HelloReply.NullableMessageFieldNumber),
                routeParameterDescriptors: routeParameterDescriptors);
            var unaryServerCallHandler = CreateCallHandler(
                invoker,
                descriptorInfo: descriptorInfo);
            var httpContext = TestHelpers.CreateHttpContext();
            httpContext.Request.RouteValues["name"] = "Doesn't matter";

            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var sr = new StreamReader(httpContext.Response.Body);
            var content = sr.ReadToEnd();

            Assert.Equal("null", content);
        }

        [Fact]
        public async Task HandleCallAsync_ResponseBodySetToRepeatedField_ArrayReturned()
        {
            // Arrange
            HelloRequest? request = null;
            UnaryServerMethod<HttpApiGreeterService, HelloRequest, HelloReply> invoker = (s, r, c) =>
            {
                request = r;
                return Task.FromResult(new HelloReply { Values = { "One", "Two", "" } });
            };

            var unaryServerCallHandler = CreateCallHandler(
                invoker,
                descriptorInfo: TestHelpers.CreateDescriptorInfo(responseBodyDescriptor: HelloReply.Descriptor.FindFieldByNumber(HelloReply.ValuesFieldNumber)));
            var httpContext = TestHelpers.CreateHttpContext();

            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            Assert.NotNull(request);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var responseJson = JsonDocument.Parse(httpContext.Response.Body);
            Assert.Equal(JsonValueKind.Array, responseJson.RootElement.ValueKind);
            Assert.Equal("One", responseJson.RootElement[0].GetString());
            Assert.Equal("Two", responseJson.RootElement[1].GetString());
            Assert.Equal("", responseJson.RootElement[2].GetString());
        }

        [Fact]
        public async Task HandleCallAsync_RootBodySet_SetOnRequestMessage()
        {
            // Arrange
            HelloRequest? request = null;
            UnaryServerMethod<HttpApiGreeterService, HelloRequest, HelloReply> invoker = (s, r, c) =>
            {
                request = r;
                return Task.FromResult(new HelloReply { Message = $"Hello {r.Name}" });
            };

            var unaryServerCallHandler = CreateCallHandler(
                invoker,
                descriptorInfo: TestHelpers.CreateDescriptorInfo(bodyDescriptor: HelloRequest.Descriptor));
            var httpContext = TestHelpers.CreateHttpContext();
            httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonFormatter.Default.Format(new HelloRequest
            {
                Name = "TestName!"
            })));
            httpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
            {
                ["name"] = "QueryStringTestName!",
                ["sub.subfield"] = "QueryStringTestSubfield!"
            });
            httpContext.Request.ContentType = "application/json";

            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            Assert.NotNull(request);
            Assert.Equal("TestName!", request!.Name);
            Assert.Null(request!.Sub);
        }

        [Fact]
        public async Task HandleCallAsync_SubBodySet_SetOnRequestMessage()
        {
            // Arrange
            HelloRequest? request = null;
            UnaryServerMethod<HttpApiGreeterService, HelloRequest, HelloReply> invoker = (s, r, c) =>
            {
                request = r;
                return Task.FromResult(new HelloReply { Message = $"Hello {r.Name}" });
            };

            ServiceDescriptorHelpers.TryResolveDescriptors(HelloRequest.Descriptor, "sub", out var bodyFieldDescriptors);

            var descriptorInfo = TestHelpers.CreateDescriptorInfo(
                bodyDescriptor: HelloRequest.Types.SubMessage.Descriptor,
                bodyFieldDescriptors: bodyFieldDescriptors);
            var unaryServerCallHandler = CreateCallHandler(
                invoker,
                descriptorInfo);
            var httpContext = TestHelpers.CreateHttpContext();
            httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonFormatter.Default.Format(new HelloRequest.Types.SubMessage
            {
                Subfield = "Subfield!"
            })));
            httpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
            {
                ["name"] = "QueryStringTestName!",
                ["sub.subfield"] = "QueryStringTestSubfield!",
                ["sub.subfields"] = "QueryStringTestSubfields!"
            });
            httpContext.Request.ContentType = "application/json";

            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            Assert.NotNull(request);
            Assert.Equal("QueryStringTestName!", request!.Name);
            Assert.Equal("Subfield!", request!.Sub.Subfield);
            Assert.Empty(request!.Sub.Subfields);
        }

        [Fact]
        public async Task HandleCallAsync_SubRepeatedBodySet_SetOnRequestMessage()
        {
            // Arrange
            HelloRequest? request = null;
            UnaryServerMethod<HttpApiGreeterService, HelloRequest, HelloReply> invoker = (s, r, c) =>
            {
                request = r;
                return Task.FromResult(new HelloReply { Message = $"Hello {r.Name}" });
            };

            ServiceDescriptorHelpers.TryResolveDescriptors(HelloRequest.Descriptor, "repeated_strings", out var bodyFieldDescriptors);

            var descriptorInfo = TestHelpers.CreateDescriptorInfo(
                bodyDescriptor: HelloRequest.Types.SubMessage.Descriptor,
                bodyDescriptorRepeated: true,
                bodyFieldDescriptors: bodyFieldDescriptors);
            var unaryServerCallHandler = CreateCallHandler(
                invoker,
                descriptorInfo);
            var httpContext = TestHelpers.CreateHttpContext();

            var sdf = new RepeatedField<string>
            {
                "One",
                "Two",
                "Three"
            };

            var sw = new StringWriter();
            JsonFormatter.Default.WriteValue(sw, sdf);

            httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(sw.ToString()));
            httpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
            {
                ["name"] = "QueryStringTestName!",
                ["sub.subfield"] = "QueryStringTestSubfield!",
                ["sub.subfields"] = "QueryStringTestSubfields!"
            });
            httpContext.Request.ContentType = "application/json";

            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            Assert.NotNull(request);
            Assert.Equal("QueryStringTestName!", request!.Name);
            Assert.Equal("QueryStringTestSubfield!", request!.Sub.Subfield);
            Assert.Equal(3, request!.RepeatedStrings.Count);
            Assert.Equal("One", request!.RepeatedStrings[0]);
            Assert.Equal("Two", request!.RepeatedStrings[1]);
            Assert.Equal("Three", request!.RepeatedStrings[2]);
        }

        [Fact]
        public async Task HandleCallAsync_SubSubRepeatedBodySet_SetOnRequestMessage()
        {
            // Arrange
            HelloRequest? request = null;
            UnaryServerMethod<HttpApiGreeterService, HelloRequest, HelloReply> invoker = (s, r, c) =>
            {
                request = r;
                return Task.FromResult(new HelloReply { Message = $"Hello {r.Name}" });
            };

            ServiceDescriptorHelpers.TryResolveDescriptors(HelloRequest.Descriptor, "sub.subfields", out var bodyFieldDescriptors);

            var descriptorInfo = TestHelpers.CreateDescriptorInfo(
                bodyDescriptor: HelloRequest.Types.SubMessage.Descriptor,
                bodyDescriptorRepeated: true,
                bodyFieldDescriptors: bodyFieldDescriptors);
            var unaryServerCallHandler = CreateCallHandler(
                invoker,
                descriptorInfo);
            var httpContext = TestHelpers.CreateHttpContext();

            var sdf = new RepeatedField<string>
            {
                "One",
                "Two",
                "Three"
            };

            var sw = new StringWriter();
            JsonFormatter.Default.WriteValue(sw, sdf);

            httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(sw.ToString()));
            httpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
            {
                ["name"] = "QueryStringTestName!",
                ["sub.subfield"] = "QueryStringTestSubfield!" // Not bound because query can't be applied to fields that are covered by body
            });
            httpContext.Request.ContentType = "application/json";

            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            Assert.NotNull(request);
            Assert.Equal("QueryStringTestName!", request!.Name);
            Assert.Equal("QueryStringTestSubfield!", request!.Sub.Subfield);
            Assert.Equal(3, request!.Sub.Subfields.Count);
        }

        [Fact]
        public async Task HandleCallAsync_MatchingQueryStringValues_SetOnRequestMessage()
        {
            // Arrange
            HelloRequest? request = null;
            UnaryServerMethod<HttpApiGreeterService, HelloRequest, HelloReply> invoker = (s, r, c) =>
            {
                request = r;
                return Task.FromResult(new HelloReply());
            };

            var unaryServerCallHandler = CreateCallHandler(invoker);
            var httpContext = TestHelpers.CreateHttpContext();
            httpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
            {
                ["name"] = "TestName!",
                ["sub.subfield"] = "TestSubfield!"
            });

            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            Assert.NotNull(request);
            Assert.Equal("TestName!", request!.Name);
            Assert.Equal("TestSubfield!", request!.Sub.Subfield);
        }

        [Fact]
        public async Task HandleCallAsync_SuccessfulResponse_DefaultValuesInResponseJson()
        {
            // Arrange
            HelloRequest? request = null;
            UnaryServerMethod<HttpApiGreeterService, HelloRequest, HelloReply> invoker = (s, r, c) =>
            {
                request = r;
                return Task.FromResult(new HelloReply());
            };

            var unaryServerCallHandler = CreateCallHandler(invoker);
            var httpContext = TestHelpers.CreateHttpContext();
            httpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
            {
                ["name"] = "TestName!"
            });

            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            Assert.NotNull(request);
            Assert.Equal("TestName!", request!.Name);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var responseJson = JsonDocument.Parse(httpContext.Response.Body);
            Assert.Equal("", responseJson.RootElement.GetProperty("message").GetString());
        }

        [Theory]
        [InlineData("{malformed_json}", "Request JSON payload is not correctly formatted.")]
        [InlineData("{\"name\": 1234}", "Request JSON payload is not correctly formatted.")]
        //[InlineData("{\"abcd\": 1234}", "Unknown field: abcd")]
        public async Task HandleCallAsync_MalformedRequestBody_BadRequestReturned(string json, string expectedError)
        {
            // Arrange
            UnaryServerMethod<HttpApiGreeterService, HelloRequest, HelloReply> invoker = (s, r, c) =>
            {
                return Task.FromResult(new HelloReply());
            };

            var unaryServerCallHandler = CreateCallHandler(
                invoker,
                descriptorInfo: TestHelpers.CreateDescriptorInfo(bodyDescriptor: HelloRequest.Descriptor));
            var httpContext = TestHelpers.CreateHttpContext();
            httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
            httpContext.Request.ContentType = "application/json";
            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            Assert.Equal(400, httpContext.Response.StatusCode);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var responseJson = JsonDocument.Parse(httpContext.Response.Body);
            Assert.Equal(expectedError, responseJson.RootElement.GetProperty("message").GetString());
            Assert.Equal(expectedError, responseJson.RootElement.GetProperty("error").GetString());
            Assert.Equal((int)StatusCode.InvalidArgument, responseJson.RootElement.GetProperty("code").GetInt32());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("text/html")]
        public async Task HandleCallAsync_BadContentType_BadRequestReturned(string contentType)
        {
            // Arrange
            UnaryServerMethod<HttpApiGreeterService, HelloRequest, HelloReply> invoker = (s, r, c) =>
            {
                return Task.FromResult(new HelloReply());
            };

            var unaryServerCallHandler = CreateCallHandler(
                invoker,
                descriptorInfo: TestHelpers.CreateDescriptorInfo(bodyDescriptor: HelloRequest.Descriptor));
            var httpContext = TestHelpers.CreateHttpContext();
            httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{}"));
            httpContext.Request.ContentType = contentType;
            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            Assert.Equal(400, httpContext.Response.StatusCode);

            var expectedError = "Request content-type of application/json is required.";
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var responseJson = JsonDocument.Parse(httpContext.Response.Body);
            Assert.Equal(expectedError, responseJson.RootElement.GetProperty("message").GetString());
            Assert.Equal(expectedError, responseJson.RootElement.GetProperty("error").GetString());
            Assert.Equal((int)StatusCode.InvalidArgument, responseJson.RootElement.GetProperty("code").GetInt32());
        }

        [Fact]
        public async Task HandleCallAsync_RpcExceptionReturned_StatusReturned()
        {
            // Arrange
            UnaryServerMethod<HttpApiGreeterService, HelloRequest, HelloReply> invoker = (s, r, c) =>
            {
                return Task.FromException<HelloReply>(new RpcException(new Status(StatusCode.Unauthenticated, "Detail!"), "Message!"));
            };

            var unaryServerCallHandler = CreateCallHandler(invoker);
            var httpContext = TestHelpers.CreateHttpContext();

            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            Assert.Equal(401, httpContext.Response.StatusCode);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var responseJson = JsonDocument.Parse(httpContext.Response.Body);
            Assert.Equal("Detail!", responseJson.RootElement.GetProperty("message").GetString());
            Assert.Equal("Detail!", responseJson.RootElement.GetProperty("error").GetString());
            Assert.Equal((int)StatusCode.Unauthenticated, responseJson.RootElement.GetProperty("code").GetInt32());
        }

        [Fact]
        public async Task HandleCallAsync_RpcExceptionThrown_StatusReturned()
        {
            // Arrange
            UnaryServerMethod<HttpApiGreeterService, HelloRequest, HelloReply> invoker = (s, r, c) =>
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Detail!"), "Message!");
            };

            var unaryServerCallHandler = CreateCallHandler(invoker);
            var httpContext = TestHelpers.CreateHttpContext();

            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            Assert.Equal(401, httpContext.Response.StatusCode);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var responseJson = JsonDocument.Parse(httpContext.Response.Body);
            Assert.Equal("Detail!", responseJson.RootElement.GetProperty("message").GetString());
            Assert.Equal("Detail!", responseJson.RootElement.GetProperty("error").GetString());
            Assert.Equal((int)StatusCode.Unauthenticated, responseJson.RootElement.GetProperty("code").GetInt32());
        }

        [Fact]
        public async Task HandleCallAsync_StatusSet_StatusReturned()
        {
            // Arrange
            UnaryServerMethod<HttpApiGreeterService, HelloRequest, HelloReply> invoker = (s, r, c) =>
            {
                c.Status = new Status(StatusCode.Unauthenticated, "Detail!");
                return Task.FromResult(new HelloReply());
            };

            var unaryServerCallHandler = CreateCallHandler(invoker);
            var httpContext = TestHelpers.CreateHttpContext();

            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            Assert.Equal(401, httpContext.Response.StatusCode);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var responseJson = JsonDocument.Parse(httpContext.Response.Body);
            Assert.Equal(@"Detail!", responseJson.RootElement.GetProperty("message").GetString());
            Assert.Equal(@"Detail!", responseJson.RootElement.GetProperty("error").GetString());
            Assert.Equal((int)StatusCode.Unauthenticated, responseJson.RootElement.GetProperty("code").GetInt32());
        }

        [Fact]
        public async Task HandleCallAsync_UserState_HttpContextInUserState()
        {
            object? requestHttpContext = null;

            // Arrange
            UnaryServerMethod<HttpApiGreeterService, HelloRequest, HelloReply> invoker = (s, r, c) =>
            {
                c.UserState.TryGetValue("__HttpContext", out requestHttpContext);
                return Task.FromResult(new HelloReply());
            };

            var unaryServerCallHandler = CreateCallHandler(invoker);
            var httpContext = TestHelpers.CreateHttpContext();

            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            Assert.Equal(httpContext, requestHttpContext);
        }

        [Fact]
        public async Task HandleCallAsync_HasInterceptor_InterceptorCalled()
        {
            object? interceptorRun = null;

            // Arrange
            UnaryServerMethod<HttpApiGreeterService, HelloRequest, HelloReply> invoker = (s, r, c) =>
            {
                c.UserState.TryGetValue("IntercepterRun", out interceptorRun);
                return Task.FromResult(new HelloReply());
            };

            var interceptors = new List<(Type Type, object[] Args)>();
            interceptors.Add((typeof(TestInterceptor), Args: Array.Empty<object>()));

            var unaryServerCallHandler = CreateCallHandler(invoker, interceptors: interceptors);
            var httpContext = TestHelpers.CreateHttpContext();

            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            Assert.True((bool)interceptorRun!);
        }

        public class TestInterceptor : Interceptor
        {
            public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
            {
                context.UserState["IntercepterRun"] = true;
                return base.UnaryServerHandler(request, context, continuation);
            }
        }

        [Fact]
        public async Task HandleCallAsync_GetHostAndMethodAndPeer_MatchHandler()
        {
            string? peer = null;
            string? host = null;
            string? method = null;

            // Arrange
            UnaryServerMethod<HttpApiGreeterService, HelloRequest, HelloReply> invoker = (s, r, c) =>
            {
                peer = c.Peer;
                host = c.Host;
                method = c.Method;
                return Task.FromResult(new HelloReply());
            };

            var unaryServerCallHandler = CreateCallHandler(invoker);
            var httpContext = TestHelpers.CreateHttpContext();

            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            Assert.Equal("ipv4:127.0.0.1:0", peer);
            Assert.Equal("localhost", host);
            Assert.Equal("/ServiceName/TestMethodName", method);
        }

        [Fact]
        public async Task HandleCallAsync_ExceptionThrown_StatusReturned()
        {
            // Arrange
            UnaryServerMethod<HttpApiGreeterService, HelloRequest, HelloReply> invoker = (s, r, c) =>
            {
                throw new InvalidOperationException("Exception!");
            };

            var unaryServerCallHandler = CreateCallHandler(invoker);
            var httpContext = TestHelpers.CreateHttpContext();

            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            Assert.Equal(500, httpContext.Response.StatusCode);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var responseJson = JsonDocument.Parse(httpContext.Response.Body);
            Assert.Equal("Exception was thrown by handler.", responseJson.RootElement.GetProperty("message").GetString());
            Assert.Equal("Exception was thrown by handler.", responseJson.RootElement.GetProperty("error").GetString());
            Assert.Equal((int)StatusCode.Unknown, responseJson.RootElement.GetProperty("code").GetInt32());
        }

        [Fact]
        public async Task HandleCallAsync_MatchingRepeatedQueryStringValues_SetOnRequestMessage()
        {
            // Arrange
            HelloRequest? request = null;
            UnaryServerMethod<HttpApiGreeterService, HelloRequest, HelloReply> invoker = (s, r, c) =>
            {
                request = r;
                return Task.FromResult(new HelloReply());
            };

            var unaryServerCallHandler = CreateCallHandler(invoker);
            var httpContext = TestHelpers.CreateHttpContext();
            httpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
            {
                ["sub.subfields"] = new StringValues(new[] { "TestSubfields1!", "TestSubfields2!" })
            });

            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            Assert.NotNull(request);
            Assert.Equal(2, request!.Sub.Subfields.Count);
            Assert.Equal("TestSubfields1!", request!.Sub.Subfields[0]);
            Assert.Equal("TestSubfields2!", request!.Sub.Subfields[1]);
        }

        [Fact]
        public async Task HandleCallAsync_DataTypes_SetOnRequestMessage()
        {
            // Arrange
            HelloRequest? request = null;
            UnaryServerMethod<HttpApiGreeterService, HelloRequest, HelloReply> invoker = (s, r, c) =>
            {
                request = r;
                return Task.FromResult(new HelloReply());
            };

            var unaryServerCallHandler = CreateCallHandler(invoker);
            var httpContext = TestHelpers.CreateHttpContext();
            httpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
            {
                ["data.single_int32"] = "1",
                ["data.single_int64"] = "2",
                ["data.single_uint32"] = "3",
                ["data.single_uint64"] = "4",
                ["data.single_sint32"] = "5",
                ["data.single_sint64"] = "6",
                ["data.single_fixed32"] = "7",
                ["data.single_fixed64"] = "8",
                ["data.single_sfixed32"] = "9",
                ["data.single_sfixed64"] = "10",
                ["data.single_float"] = "11.1",
                ["data.single_double"] = "12.1",
                ["data.single_bool"] = "true",
                ["data.single_string"] = "A string",
                ["data.single_bytes"] = Convert.ToBase64String(new byte[] { 1, 2, 3 }),
                ["data.single_enum"] = "FOO",
                ["data.single_message.subfield"] = "Nested string"
            });

            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            Assert.NotNull(request);
            Assert.Equal(1, request!.Data.SingleInt32);
            Assert.Equal(2, request!.Data.SingleInt64);
            Assert.Equal((uint)3, request!.Data.SingleUint32);
            Assert.Equal((ulong)4, request!.Data.SingleUint64);
            Assert.Equal(5, request!.Data.SingleSint32);
            Assert.Equal(6, request!.Data.SingleSint64);
            Assert.Equal((uint)7, request!.Data.SingleFixed32);
            Assert.Equal((ulong)8, request!.Data.SingleFixed64);
            Assert.Equal(9, request!.Data.SingleSfixed32);
            Assert.Equal(10, request!.Data.SingleSfixed64);
            Assert.Equal(11.1, request!.Data.SingleFloat, 3);
            Assert.Equal(12.1, request!.Data.SingleDouble, 3);
            Assert.True(request!.Data.SingleBool);
            Assert.Equal("A string", request!.Data.SingleString);
            Assert.Equal(new byte[] { 1, 2, 3 }, request!.Data.SingleBytes.ToByteArray());
            Assert.Equal(HelloRequest.Types.DataTypes.Types.NestedEnum.Foo, request!.Data.SingleEnum);
            Assert.Equal("Nested string", request!.Data.SingleMessage.Subfield);
        }

        [Fact]
        public async Task HandleCallAsync_GetHttpContext_ReturnValue()
        {
            HttpContext? httpContext = null;
            var request = await ExecuteUnaryHandler(handler: (r, c) =>
            {
                httpContext = c.GetHttpContext();
                return Task.FromResult(new HelloReply());
            });

            // Assert
            Assert.NotNull(httpContext);
        }

        [Fact]
        public async Task HandleCallAsync_ServerCallContextFeature_ReturnValue()
        {
            IServerCallContextFeature? feature = null;
            var request = await ExecuteUnaryHandler(handler: (r, c) =>
            {
                feature = c.GetHttpContext().Features.Get<IServerCallContextFeature>();
                return Task.FromResult(new HelloReply());
            });

            // Assert
            Assert.NotNull(feature);
        }

        [Theory]
        [InlineData("0", HelloRequest.Types.DataTypes.Types.NestedEnum.Unspecified)]
        [InlineData("1", HelloRequest.Types.DataTypes.Types.NestedEnum.Foo)]
        [InlineData("2", HelloRequest.Types.DataTypes.Types.NestedEnum.Bar)]
        [InlineData("3", HelloRequest.Types.DataTypes.Types.NestedEnum.Baz)]
        [InlineData("-1", HelloRequest.Types.DataTypes.Types.NestedEnum.Neg)]
        public async Task HandleCallAsync_IntegerEnum_SetOnRequestMessage(string value, HelloRequest.Types.DataTypes.Types.NestedEnum expectedEnum)
        {
            var request = await ExecuteUnaryHandler(httpContext =>
            {
                httpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    ["data.single_enum"] = value
                });
            });

            // Assert
            Assert.Equal(expectedEnum, request.Data.SingleEnum);
        }

        [Theory]
        [InlineData("99")]
        [InlineData("INVALID")]
        public async Task HandleCallAsync_InvalidEnum_Error(string value)
        {
            await ExecuteUnaryHandler(httpContext =>
            {
                httpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    ["data.single_enum"] = value
                });
            });

            var exceptionWrite = TestSink.Writes.Single(w => w.EventId.Name == "ErrorExecutingServiceMethod");
            Assert.Equal($"Invalid value '{value}' for enum type NestedEnum.", exceptionWrite.Exception.Message);
        }

        private async Task<HelloRequest> ExecuteUnaryHandler(
            Action<HttpContext>? configureHttpContext = null,
            Func<HelloRequest, ServerCallContext, Task<HelloReply>>? handler = null)
        {
            // Arrange
            HelloRequest? request = null;
            UnaryServerMethod<HttpApiGreeterService, HelloRequest, HelloReply> invoker = (s, r, c) =>
            {
                request = r;
                return handler != null ? handler(r, c) : Task.FromResult(new HelloReply());
            };

            var unaryServerCallHandler = CreateCallHandler(invoker);
            var httpContext = TestHelpers.CreateHttpContext();
            configureHttpContext?.Invoke(httpContext);

            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);
            return request!;
        }

        [Fact]
        public async Task HandleCallAsync_Wrappers_SetOnRequestMessage()
        {
            // Arrange
            HelloRequest? request = null;
            UnaryServerMethod<HttpApiGreeterService, HelloRequest, HelloReply> invoker = (s, r, c) =>
            {
                request = r;
                return Task.FromResult(new HelloReply());
            };

            var unaryServerCallHandler = CreateCallHandler(invoker);
            var httpContext = TestHelpers.CreateHttpContext();
            httpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
            {
                ["wrappers.string_value"] = "1",
                ["wrappers.int32_value"] = "2",
                ["wrappers.int64_value"] = "3",
                ["wrappers.float_value"] = "4.1",
                ["wrappers.double_value"] = "5.1",
                ["wrappers.bool_value"] = "true",
                ["wrappers.uint32_value"] = "7",
                ["wrappers.uint64_value"] = "8",
                ["wrappers.bytes_value"] = Convert.ToBase64String(new byte[] { 1, 2, 3 })
            });

            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            Assert.NotNull(request);
            Assert.Equal("1", request!.Wrappers.StringValue);
            Assert.Equal(2, request!.Wrappers.Int32Value);
            Assert.Equal(3, request!.Wrappers.Int64Value);
            Assert.Equal(4.1, request!.Wrappers.FloatValue.GetValueOrDefault(), 3);
            Assert.Equal(5.1, request!.Wrappers.DoubleValue.GetValueOrDefault(), 3);
            Assert.Equal(true, request!.Wrappers.BoolValue);
            Assert.Equal((uint)7, request!.Wrappers.Uint32Value.GetValueOrDefault());
            Assert.Equal((ulong)8, request!.Wrappers.Uint64Value.GetValueOrDefault());
            Assert.Equal(new byte[] { 1, 2, 3 }, request!.Wrappers.BytesValue.ToByteArray());
        }

        [Fact]
        public async Task HandleCallAsync_Any_Success()
        {
            // Arrange
            HelloRequest? request = null;
            UnaryServerMethod<HttpApiGreeterService, HelloRequest, HelloReply> invoker = (s, r, c) =>
            {
                request = r;
                return Task.FromResult(new HelloReply
                {
                    AnyMessage = Any.Pack(new StringValue { Value = "A value!" })
                });
            };

            var typeRegistry = TypeRegistry.FromMessages(StringValue.Descriptor, Int32Value.Descriptor);
            var jsonFormatter = new JsonFormatter(new JsonFormatter.Settings(formatDefaultValues: true, typeRegistry));

            var unaryServerCallHandler = CreateCallHandler(
                invoker,
                descriptorInfo: TestHelpers.CreateDescriptorInfo(bodyDescriptor: HelloRequest.Descriptor),
                httpApiOptions: new GrpcHttpApiOptions
                {
                    JsonSettings = new JsonSettings
                    {
                        TypeRegistry = typeRegistry
                    }
                });
            var httpContext = TestHelpers.CreateHttpContext();
            var requestJson = jsonFormatter.Format(new HelloRequest
            {
                Name = "Test",
                AnyMessage = Any.Pack(new Int32Value { Value = 123 })
            });
            httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestJson));
            httpContext.Request.ContentType = "application/json";

            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            Assert.NotNull(request);
            Assert.Equal("Test", request!.Name);
            Assert.Equal("type.googleapis.com/google.protobuf.Int32Value", request!.AnyMessage.TypeUrl);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var responseJson = JsonDocument.Parse(httpContext.Response.Body);

            var anyMessage = responseJson.RootElement.GetProperty("anyMessage");
            Assert.Equal("type.googleapis.com/google.protobuf.StringValue", anyMessage.GetProperty("@type").GetString());
            Assert.Equal("A value!", anyMessage.GetProperty("value").GetString());
        }

        private UnaryServerCallHandler<HttpApiGreeterService, HelloRequest, HelloReply> CreateCallHandler(
            UnaryServerMethod<HttpApiGreeterService, HelloRequest, HelloReply> invoker,
            CallHandlerDescriptorInfo? descriptorInfo = null,
            List<(Type Type, object[] Args)>? interceptors = null,
            GrpcHttpApiOptions? httpApiOptions = null)
        {
            var serviceOptions = new GrpcServiceOptions();
            if (interceptors != null)
            {
                foreach (var interceptor in interceptors)
                {
                    serviceOptions.Interceptors.Add(interceptor.Type, interceptor.Args ?? Array.Empty<object>());
                }
            }

            var unaryServerCallInvoker = new UnaryServerMethodInvoker<HttpApiGreeterService, HelloRequest, HelloReply>(
                invoker,
                CreateServiceMethod<HelloRequest, HelloReply>("TestMethodName", HelloRequest.Parser, HelloReply.Parser),
                MethodOptions.Create(new[] { serviceOptions }),
                new TestGrpcServiceActivator<HttpApiGreeterService>());

            var jsonSettings = httpApiOptions?.JsonSettings ?? new JsonSettings();

            return new UnaryServerCallHandler<HttpApiGreeterService, HelloRequest, HelloReply>(
                unaryServerCallInvoker,
                LoggerFactory,
                descriptorInfo ?? TestHelpers.CreateDescriptorInfo(),
                JsonConverterHelper.CreateSerializerOptions(jsonSettings));
        }

        public static Marshaller<TMessage> GetMarshaller<TMessage>(MessageParser<TMessage> parser) where TMessage : IMessage<TMessage> =>
            Marshallers.Create<TMessage>(r => r.ToByteArray(), data => parser.ParseFrom(data));

        public static readonly Method<HelloRequest, HelloReply> ServiceMethod = CreateServiceMethod("MethodName", HelloRequest.Parser, HelloReply.Parser);

        public static Method<TRequest, TResponse> CreateServiceMethod<TRequest, TResponse>(string methodName, MessageParser<TRequest> requestParser, MessageParser<TResponse> responseParser)
             where TRequest : IMessage<TRequest>
             where TResponse : IMessage<TResponse>
        {
            return new Method<TRequest, TResponse>(MethodType.Unary, "ServiceName", methodName, GetMarshaller(requestParser), GetMarshaller(responseParser));
        }
    }
}
