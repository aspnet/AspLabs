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
using Microsoft.AspNet.TestHost;
using Xunit;

namespace Microsoft.AspNet.Proxy.Test
{
    public class PassthroughProxyTest
    {
        [Fact]
        public async Task PassthroughGetRequest()
        {
            var options = new ProxyOptions()
            {
                Scheme = "http",
                Host = "localhost",
                Port = "3001"
            };

            options.BackChannelMessageHandler = new TestMessageHandler
            {
                Sender = req =>
                {
                    Assert.Equal(HttpMethod.Get, req.Method);
                    IEnumerable<string> hostValue;
                    req.Headers.TryGetValues("Host", out hostValue);
                    Assert.Equal("localhost:3001", hostValue.Single());
                    Assert.Equal("http://localhost:3001/", req.RequestUri.ToString());
                    var response = new HttpResponseMessage();
                    response.Content = new ByteArrayContent(new byte[0]);
                    return response;
                }
            };

            var server = TestServer.Create(app =>
            {
                app.RunProxy(options);
            });

            var request = new HttpRequestMessage(HttpMethod.Get, "");
            var responseMessage = await server.CreateClient().SendAsync(request);
            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
        }

        [Fact]
        public async Task PassthroughPostRequestNoBody()
        {
            var options = new ProxyOptions()
            {
                Scheme = "http",
                Host = "localhost",
                Port = "3002"
            };

            options.BackChannelMessageHandler = new TestMessageHandler
            {
                Sender = req =>
                {
                    IEnumerable<string> hostValue;
                    req.Headers.TryGetValues("Host", out hostValue);
                    Assert.Equal("localhost:3002", hostValue.Single());
                    Assert.Equal("http://localhost:3002/", req.RequestUri.ToString());
                    Assert.Equal(HttpMethod.Post, req.Method);
                    var response = new HttpResponseMessage();
                    response.Content = new ByteArrayContent(new byte[0]);
                    return response;
                }
            };

            var server = TestServer.Create(app =>
            {
                app.RunProxy(options);
            });

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "");
            requestMessage.Content = new ByteArrayContent(new byte[0]);
            var responseMessage = await server.CreateClient().SendAsync(requestMessage);
            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
        }

        [Fact]
        public async Task PassthroughPostRequestWithRequestBody()
        {
            var options = new ProxyOptions()
            {
                Scheme = "http",
                Host = "localhost",
                Port = "3003"
            };

            options.BackChannelMessageHandler = new TestMessageHandler
            {
                Sender = req =>
                {
                    IEnumerable<string> hostValue;
                    req.Headers.TryGetValues("Host", out hostValue);
                    Assert.Equal("localhost:3003", hostValue.Single());
                    Assert.Equal("http://localhost:3003/", req.RequestUri.ToString());
                    Assert.Equal(HttpMethod.Post, req.Method);
                    var content = req.Content.ReadAsStringAsync();
                    Assert.True(content.Wait(3000) && !content.IsFaulted);
                    Assert.Equal("Post Request", content.Result);
                    var response = new HttpResponseMessage(HttpStatusCode.Created);
                    response.Content = new ByteArrayContent(new byte[0]);
                    return response;
                }
            };

            var server = TestServer.Create(app =>
            {
                app.RunProxy(options);
            });

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "");
            requestMessage.Content = new StringContent("Post Request");
            var responseMessage = await server.CreateClient().SendAsync(requestMessage);
            Assert.Equal(HttpStatusCode.Created, responseMessage.StatusCode);
        }

        [Fact]
        public async Task PassthroughPostRequestWithResponseBody()
        {
            var options = new ProxyOptions()
            {
                Scheme = "http",
                Host = "localhost",
                Port = "3004"
            };

            options.BackChannelMessageHandler = new TestMessageHandler
            {
                Sender = req =>
                {
                    IEnumerable<string> hostValue;
                    req.Headers.TryGetValues("Host", out hostValue);
                    Assert.Equal("localhost:3004", hostValue.Single());
                    Assert.Equal("http://localhost:3004/", req.RequestUri.ToString());
                    Assert.Equal(HttpMethod.Post, req.Method);
                    var content = req.Content.ReadAsStringAsync();
                    Assert.True(content.Wait(3000) && !content.IsFaulted);
                    var response = new HttpResponseMessage(HttpStatusCode.Created);
                    response.Content = new StringContent("Response Content");
                    return response;
                }
            };

            var server = TestServer.Create(app =>
            {
                app.RunProxy(options);
            });

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "");
            var responseMessage = await server.CreateClient().SendAsync(requestMessage);
            var responseContent = await responseMessage.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.Created, responseMessage.StatusCode);
            Assert.Equal("Response Content", responseContent);
        }

        [Fact]
        public async Task PassthroughGetRequestWithResponseHeaders()
        {
            var options = new ProxyOptions()
            {
                Scheme = "http",
                Host = "localhost",
                Port = "3005"
            };
            options.BackChannelMessageHandler = new TestMessageHandler
            {
                Sender = req =>
                    {
                        IEnumerable<string> hostValue;
                        req.Headers.TryGetValues("Host", out hostValue);
                        Assert.Equal("localhost:3005", hostValue.Single());
                        Assert.Equal("http://localhost:3005/", req.RequestUri.ToString());
                        Assert.Equal(HttpMethod.Get, req.Method);
                        var response = new HttpResponseMessage(HttpStatusCode.Created);
                        response.Headers.Add("testHeader", "testHeaderValue");
                        response.Content = new StringContent("Response Content");
                        return response;
                    }
            };

            var server = TestServer.Create(app =>
            {
                app.RunProxy(options);
            });

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "");
            var responseMessage = await server.CreateClient().SendAsync(requestMessage);
            Assert.Equal(HttpStatusCode.Created, responseMessage.StatusCode);
            IEnumerable<string> testHeaderValue;
            responseMessage.Headers.TryGetValues("testHeader", out testHeaderValue);
            Assert.Equal("testHeaderValue", testHeaderValue.Single());
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
