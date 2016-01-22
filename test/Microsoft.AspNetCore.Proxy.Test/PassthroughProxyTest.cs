// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.TestHost;
using Xunit;

namespace Microsoft.AspNet.Proxy.Test
{
    public class ProxyTest
    {
        [Theory]
        [InlineData("GET", "3001")]
        [InlineData("HEAD", "3002")]
        [InlineData("TRACE", "3003")]
        [InlineData("DELETE", "3004")]
        public async Task PassthroughRequestsWithoutBodyWithResponseHeaders(string MethodType, string Port)
        {
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.RunProxy(new ProxyOptions
                    {
                        Scheme = "http",
                        Host = "localhost",
                        Port = Port,
                        BackChannelMessageHandler = new TestMessageHandler
                        {
                            Sender = req =>
                            {
                                IEnumerable<string> hostValue;
                                req.Headers.TryGetValues("Host", out hostValue);
                                Assert.Equal("localhost:" + Port, hostValue.Single());
                                Assert.Equal("http://localhost:" + Port + "/", req.RequestUri.ToString());
                                Assert.Equal(new HttpMethod(MethodType), req.Method);
                                var response = new HttpResponseMessage(HttpStatusCode.Created);
                                response.Headers.Add("testHeader", "testHeaderValue");
                                response.Content = new StringContent("Response Body");
                                return response;
                            }
                        }
                    });
                });
            var server = new TestServer(builder);

            var requestMessage = new HttpRequestMessage(new HttpMethod(MethodType), "");
            var responseMessage = await server.CreateClient().SendAsync(requestMessage);
            Assert.Equal(HttpStatusCode.Created, responseMessage.StatusCode);
            var responseContent = responseMessage.Content.ReadAsStringAsync();
            Assert.True(responseContent.Wait(3000) && !responseContent.IsFaulted);
            Assert.Equal("Response Body", responseContent.Result);
            IEnumerable<string> testHeaderValue;
            responseMessage.Headers.TryGetValues("testHeader", out testHeaderValue);
            Assert.Equal("testHeaderValue", testHeaderValue.Single());
        }

        [Theory]
        [InlineData("POST", "3005")]
        [InlineData("PUT", "3006")]
        [InlineData("OPTIONS", "3007")]
        [InlineData("NewHttpMethod", "3008")]
        public async Task PassthroughReuestWithBody(string MethodType, string Port)
        {
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.RunProxy(new ProxyOptions
                    {
                        Scheme = "http",
                        Host = "localhost",
                        Port = Port,
                        BackChannelMessageHandler = new TestMessageHandler
                        {
                            Sender = req =>
                            {
                                IEnumerable<string> hostValue;
                                req.Headers.TryGetValues("Host", out hostValue);
                                Assert.Equal("localhost:" + Port, hostValue.Single());
                                Assert.Equal("http://localhost:" + Port + "/", req.RequestUri.ToString());
                                Assert.Equal(new HttpMethod(MethodType), req.Method);
                                var content = req.Content.ReadAsStringAsync();
                                Assert.True(content.Wait(3000) && !content.IsFaulted);
                                Assert.Equal("Request Body", content.Result);
                                var response = new HttpResponseMessage(HttpStatusCode.Created);
                                response.Headers.Add("testHeader", "testHeaderValue");
                                response.Content = new StringContent("Response Body");
                                return response;
                            }
                        }
                    });
                });
            var server = new TestServer(builder);

            var requestMessage = new HttpRequestMessage(new HttpMethod(MethodType), "");
            requestMessage.Content = new StringContent("Request Body");
            var responseMessage = await server.CreateClient().SendAsync(requestMessage);
            var responseContent = responseMessage.Content.ReadAsStringAsync();
            Assert.True(responseContent.Wait(3000) && !responseContent.IsFaulted);
            Assert.Equal("Response Body", responseContent.Result);
            Assert.Equal(HttpStatusCode.Created, responseMessage.StatusCode);
        }

        private class TestMessageHandler : HttpMessageHandler
        {
            public Func<HttpRequestMessage, HttpResponseMessage> Sender { get; set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                if (Sender != null)
                {
                    return Task.FromResult(Sender(request));
                }

                return Task.FromResult<HttpResponseMessage>(null);
            }
        }
    }
}
