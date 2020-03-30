// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Grpc.AspNetCore.Server.Model;
using Grpc.Core;
using Grpc.Shared.Server;
using Grpc.Tests.Shared;
using HttpApi;
using Microsoft.AspNetCore.Grpc.HttpApi;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;

namespace Grpc.AspNetCore.Server.Tests.HttpApi
{
    [TestFixture]
    public class UnaryServerCallHandlerTests
    {
        [Test]
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
            var unaryServerCallHandler = CreateCallHandler(invoker, routeParameterDescriptors: routeParameterDescriptors);
            var httpContext = CreateHttpContext();
            httpContext.Request.RouteValues["name"] = "TestName!";
            httpContext.Request.RouteValues["sub.subfield"] = "Subfield!";

            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            Assert.IsNotNull(request);
            Assert.AreEqual("TestName!", request!.Name);
            Assert.AreEqual("Subfield!", request!.Sub.Subfield);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var responseJson = JsonDocument.Parse(httpContext.Response.Body);
            Assert.AreEqual("Hello TestName!", responseJson.RootElement.GetProperty("message").GetString());
        }

        [Test]
        public async Task HandleCallAsync_ResponseBodySet_ResponseReturned()
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
                ["name"] = new List<FieldDescriptor>(new[] { HelloRequest.Descriptor.FindFieldByNumber(HelloRequest.NameFieldNumber) })
            };
            var unaryServerCallHandler = CreateCallHandler(
                invoker,
                HelloReply.Descriptor.FindFieldByNumber(HelloReply.MessageFieldNumber),
                routeParameterDescriptors);
            var httpContext = CreateHttpContext();
            httpContext.Request.RouteValues["name"] = "TestName!";

            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            Assert.IsNotNull(request);
            Assert.AreEqual("TestName!", request!.Name);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var responseJson = JsonDocument.Parse(httpContext.Response.Body);
            Assert.AreEqual("Hello TestName!", responseJson.RootElement.GetString());
        }

        [Test]
        public async Task HandleCallAsync_ResponseBodySetToRepeatedField_ArrayReturned()
        {
            // Arrange
            HelloRequest? request = null;
            UnaryServerMethod<HttpApiGreeterService, HelloRequest, HelloReply> invoker = (s, r, c) =>
            {
                request = r;
                return Task.FromResult(new HelloReply { Values = { "One", "Two", "Three" } });
            };

            var unaryServerCallHandler = CreateCallHandler(
                invoker,
                HelloReply.Descriptor.FindFieldByNumber(HelloReply.ValuesFieldNumber));
            var httpContext = CreateHttpContext();

            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            Assert.IsNotNull(request);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var responseJson = JsonDocument.Parse(httpContext.Response.Body);
            Assert.AreEqual(JsonValueKind.Array, responseJson.RootElement.ValueKind);
            Assert.AreEqual("One", responseJson.RootElement[0].GetString());
            Assert.AreEqual("Two", responseJson.RootElement[1].GetString());
            Assert.AreEqual("Three", responseJson.RootElement[2].GetString());
        }

        [Test]
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
                bodyDescriptor: HelloRequest.Descriptor);
            var httpContext = CreateHttpContext();
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
            Assert.IsNotNull(request);
            Assert.AreEqual("TestName!", request!.Name);
            Assert.AreEqual(null, request!.Sub);
        }

        [Test]
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

            var unaryServerCallHandler = CreateCallHandler(
                invoker,
                bodyDescriptor: HelloRequest.Types.SubMessage.Descriptor,
                bodyFieldDescriptors: bodyFieldDescriptors);
            var httpContext = CreateHttpContext();
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
            Assert.IsNotNull(request);
            Assert.AreEqual("QueryStringTestName!", request!.Name);
            Assert.AreEqual("Subfield!", request!.Sub.Subfield);
            Assert.AreEqual(0, request!.Sub.Subfields.Count);
        }

        [Test]
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

            var unaryServerCallHandler = CreateCallHandler(
                invoker,
                bodyDescriptor: HelloRequest.Types.SubMessage.Descriptor,
                bodyDescriptorRepeated: true,
                bodyFieldDescriptors: bodyFieldDescriptors);
            var httpContext = CreateHttpContext();

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
            Assert.IsNotNull(request);
            Assert.AreEqual("QueryStringTestName!", request!.Name);
            Assert.AreEqual("QueryStringTestSubfield!", request!.Sub.Subfield);
            Assert.AreEqual(3, request!.RepeatedStrings.Count);
            Assert.AreEqual("One", request!.RepeatedStrings[0]);
            Assert.AreEqual("Two", request!.RepeatedStrings[1]);
            Assert.AreEqual("Three", request!.RepeatedStrings[2]);
        }

        [Test]
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

            var unaryServerCallHandler = CreateCallHandler(
                invoker,
                bodyDescriptor: HelloRequest.Types.SubMessage.Descriptor,
                bodyDescriptorRepeated: true,
                bodyFieldDescriptors: bodyFieldDescriptors);
            var httpContext = CreateHttpContext();

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
            Assert.IsNotNull(request);
            Assert.AreEqual("QueryStringTestName!", request!.Name);
            Assert.AreEqual("QueryStringTestSubfield!", request!.Sub.Subfield);
            Assert.AreEqual(3, request!.Sub.Subfields.Count);
        }

        [Test]
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
            var httpContext = CreateHttpContext();
            httpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
            {
                ["name"] = "TestName!",
                ["sub.subfield"] = "TestSubfield!"
            });

            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            Assert.IsNotNull(request);
            Assert.AreEqual("TestName!", request!.Name);
            Assert.AreEqual("TestSubfield!", request!.Sub.Subfield);
        }

        [Test]
        public async Task HandleCallAsync_RpcExceptionReturned_StatusReturned()
        {
            // Arrange
            UnaryServerMethod<HttpApiGreeterService, HelloRequest, HelloReply> invoker = (s, r, c) =>
            {
                return Task.FromException<HelloReply>(new RpcException(new Status(StatusCode.Unauthenticated, "Detail!"), "Message!"));
            };

            var unaryServerCallHandler = CreateCallHandler(invoker);
            var httpContext = CreateHttpContext();

            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            Assert.AreEqual(401, httpContext.Response.StatusCode);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var responseJson = JsonDocument.Parse(httpContext.Response.Body);
            Assert.AreEqual("Message!", responseJson.RootElement.GetProperty("message").GetString());
            Assert.AreEqual("Message!", responseJson.RootElement.GetProperty("error").GetString());
            Assert.AreEqual((int)StatusCode.Unauthenticated, responseJson.RootElement.GetProperty("code").GetInt32());
        }

        [Test]
        public async Task HandleCallAsync_RpcExceptionThrown_StatusReturned()
        {
            // Arrange
            UnaryServerMethod<HttpApiGreeterService, HelloRequest, HelloReply> invoker = (s, r, c) =>
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Detail!"), "Message!");
            };

            var unaryServerCallHandler = CreateCallHandler(invoker);
            var httpContext = CreateHttpContext();

            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            Assert.AreEqual(401, httpContext.Response.StatusCode);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var responseJson = JsonDocument.Parse(httpContext.Response.Body);
            Assert.AreEqual("Message!", responseJson.RootElement.GetProperty("message").GetString());
            Assert.AreEqual("Message!", responseJson.RootElement.GetProperty("error").GetString());
            Assert.AreEqual((int)StatusCode.Unauthenticated, responseJson.RootElement.GetProperty("code").GetInt32());
        }

        [Test]
        public async Task HandleCallAsync_StatusSet_StatusReturned()
        {
            // Arrange
            UnaryServerMethod<HttpApiGreeterService, HelloRequest, HelloReply> invoker = (s, r, c) =>
            {
                c.Status = new Status(StatusCode.Unauthenticated, "Detail!");
                return Task.FromResult(new HelloReply());
            };

            var unaryServerCallHandler = CreateCallHandler(invoker);
            var httpContext = CreateHttpContext();

            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            Assert.AreEqual(401, httpContext.Response.StatusCode);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var responseJson = JsonDocument.Parse(httpContext.Response.Body);
            Assert.AreEqual(@"Status(StatusCode=Unauthenticated, Detail=""Detail!"")", responseJson.RootElement.GetProperty("message").GetString());
            Assert.AreEqual(@"Status(StatusCode=Unauthenticated, Detail=""Detail!"")", responseJson.RootElement.GetProperty("error").GetString());
            Assert.AreEqual((int)StatusCode.Unauthenticated, responseJson.RootElement.GetProperty("code").GetInt32());
        }

        [Test]
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
            var httpContext = CreateHttpContext();

            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            Assert.AreEqual(httpContext, requestHttpContext);
        }

        [Test]
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
            var httpContext = CreateHttpContext();

            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            Assert.AreEqual("ipv4:127.0.0.1:0", peer);
            Assert.AreEqual("localhost", host);
            Assert.AreEqual("/ServiceName/TestMethodName", method);
        }

        [Test]
        public async Task HandleCallAsync_GetCancellationToken_MatchHttpContextRequestAborted()
        {
            var cts = new CancellationTokenSource();
            CancellationToken serverCallContextCt = default;

            // Arrange
            UnaryServerMethod<HttpApiGreeterService, HelloRequest, HelloReply> invoker = (s, r, c) =>
            {
                serverCallContextCt = c.CancellationToken;
                return Task.FromResult(new HelloReply());
            };

            var unaryServerCallHandler = CreateCallHandler(invoker);
            var httpContext = CreateHttpContext(cancellationToken: cts.Token);

            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            Assert.AreEqual(cts.Token, serverCallContextCt);
        }

        [Test]
        public async Task HandleCallAsync_ExceptionThrown_StatusReturned()
        {
            // Arrange
            UnaryServerMethod<HttpApiGreeterService, HelloRequest, HelloReply> invoker = (s, r, c) =>
            {
                throw new InvalidOperationException("Exception!");
            };

            var unaryServerCallHandler = CreateCallHandler(invoker);
            var httpContext = CreateHttpContext();

            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            Assert.AreEqual(500, httpContext.Response.StatusCode);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var responseJson = JsonDocument.Parse(httpContext.Response.Body);
            Assert.AreEqual("Exception was thrown by handler.", responseJson.RootElement.GetProperty("message").GetString());
            Assert.AreEqual("Exception was thrown by handler.", responseJson.RootElement.GetProperty("error").GetString());
            Assert.AreEqual((int)StatusCode.Unknown, responseJson.RootElement.GetProperty("code").GetInt32());
        }

        [Test]
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
            var httpContext = CreateHttpContext();
            httpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
            {
                ["sub.subfields"] = new StringValues(new[] { "TestSubfields1!", "TestSubfields2!" })
            });

            // Act
            await unaryServerCallHandler.HandleCallAsync(httpContext);

            // Assert
            Assert.IsNotNull(request);
            Assert.AreEqual(2, request!.Sub.Subfields.Count);
            Assert.AreEqual("TestSubfields1!", request!.Sub.Subfields[0]);
            Assert.AreEqual("TestSubfields2!", request!.Sub.Subfields[1]);
        }

        [Test]
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
            var httpContext = CreateHttpContext();
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
            Assert.IsNotNull(request);
            Assert.AreEqual(1, request!.Data.SingleInt32);
            Assert.AreEqual(2, request!.Data.SingleInt64);
            Assert.AreEqual(3, request!.Data.SingleUint32);
            Assert.AreEqual(4, request!.Data.SingleUint64);
            Assert.AreEqual(5, request!.Data.SingleSint32);
            Assert.AreEqual(6, request!.Data.SingleSint64);
            Assert.AreEqual(7, request!.Data.SingleFixed32);
            Assert.AreEqual(8, request!.Data.SingleFixed64);
            Assert.AreEqual(9, request!.Data.SingleSfixed32);
            Assert.AreEqual(10, request!.Data.SingleSfixed64);
            Assert.AreEqual(11.1, request!.Data.SingleFloat, 0.001);
            Assert.AreEqual(12.1, request!.Data.SingleDouble, 0.001);
            Assert.AreEqual(true, request!.Data.SingleBool);
            Assert.AreEqual("A string", request!.Data.SingleString);
            Assert.AreEqual(new byte[] { 1, 2, 3 }, request!.Data.SingleBytes.ToByteArray());
            Assert.AreEqual(HelloRequest.Types.DataTypes.Types.NestedEnum.Foo, request!.Data.SingleEnum);
            Assert.AreEqual("Nested string", request!.Data.SingleMessage.Subfield);
        }

        [Test]
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
            var httpContext = CreateHttpContext();
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
            Assert.IsNotNull(request);
            Assert.AreEqual("1", request!.Wrappers.StringValue);
            Assert.AreEqual(2, request!.Wrappers.Int32Value);
            Assert.AreEqual(3, request!.Wrappers.Int64Value);
            Assert.AreEqual(4.1, request!.Wrappers.FloatValue, 0.001);
            Assert.AreEqual(5.1, request!.Wrappers.DoubleValue, 0.001);
            Assert.AreEqual(true, request!.Wrappers.BoolValue);
            Assert.AreEqual(7, request!.Wrappers.Uint32Value);
            Assert.AreEqual(8, request!.Wrappers.Uint64Value);
            Assert.AreEqual(new byte[] { 1, 2, 3 }, request!.Wrappers.BytesValue.ToByteArray());
        }

        private static DefaultHttpContext CreateHttpContext(CancellationToken cancellationToken = default)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<HttpApiGreeterService>();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Host = new HostString("localhost");
            httpContext.RequestServices = serviceProvider;
            httpContext.Response.Body = new MemoryStream();
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
            httpContext.Features.Set<IHttpRequestLifetimeFeature>(new HttpRequestLifetimeFeature(cancellationToken));
            return httpContext;
        }

        private class HttpRequestLifetimeFeature : IHttpRequestLifetimeFeature
        {
            public HttpRequestLifetimeFeature(CancellationToken cancellationToken)
            {
                RequestAborted = cancellationToken;
            }

            public CancellationToken RequestAborted { get; set; }

            public void Abort()
            {
            }
        }

        private static UnaryServerCallHandler<HttpApiGreeterService, HelloRequest, HelloReply> CreateCallHandler(
            UnaryServerMethod<HttpApiGreeterService, HelloRequest, HelloReply> invoker,
            FieldDescriptor? responseBodyDescriptor = null,
            Dictionary<string, List<FieldDescriptor>>? routeParameterDescriptors = null,
            MessageDescriptor? bodyDescriptor = null,
            bool? bodyDescriptorRepeated = null,
            List<FieldDescriptor>? bodyFieldDescriptors = null)
        {
            var unaryServerCallInvoker = new UnaryServerMethodInvoker<HttpApiGreeterService, HelloRequest, HelloReply>(
                invoker,
                CreateServiceMethod<HelloRequest, HelloReply>("TestMethodName", HelloRequest.Parser, HelloReply.Parser),
                Shared.Server.MethodOptions.Create(new[] { new GrpcServiceOptions() }),
                new TestGrpcServiceActivator<HttpApiGreeterService>());
            
            return new UnaryServerCallHandler<HttpApiGreeterService, HelloRequest, HelloReply>(
                unaryServerCallInvoker,
                responseBodyDescriptor,
                bodyDescriptor,
                bodyDescriptorRepeated ?? false,
                bodyFieldDescriptors,
                routeParameterDescriptors ?? new Dictionary<string, List<FieldDescriptor>>());
        }

        private class HttpApiGreeterService : HttpApiGreeter.HttpApiGreeterBase
        {
            public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
            {
                return base.SayHello(request, context);
            }
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
