// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.TestUtilities.Mocks;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class WebHookManagerTests : IDisposable
    {
        private const string TestUser = "TestUser";
        private const string SerializedWebHook = "{\r\n  \"Id\": \"1234567890\",\r\n  \"Attempt\": 1,\r\n  \"Properties\": {\r\n    \"p1\": \"pv1\"\r\n  },\r\n  \"Notifications\": [\r\n    {\r\n      \"Action\": \"a1\",\r\n      \"d1\": \"dv1\"\r\n    },\r\n    {\r\n      \"Action\": \"a1\",\r\n      \"d2\": \"http://localhost/\"\r\n    }\r\n  ]\r\n}";
        private const string WebHookSignature = "sha256=69941A3C522CE0B52B5F08BD23309D4356422FEFF99A3398062A7C015B9FD48D";

        private readonly HttpClient _httpClient;
        private readonly Mock<IWebHookStore> _storeMock;
        private readonly ExecutionDataflowBlockOptions _options;
        private readonly HttpMessageHandlerMock _handlerMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly HttpResponseMessage _response;

        private WebHookManager _manager;

        public WebHookManagerTests()
        {
            _handlerMock = new HttpMessageHandlerMock();
            _httpClient = new HttpClient(_handlerMock);
            _storeMock = new Mock<IWebHookStore>();
            _storeMock.Setup<Task<ICollection<WebHook>>>(s => s.QueryWebHooksAsync(TestUser, new[] { "a1" }))
                .ReturnsAsync(new Collection<WebHook> { CreateWebHook() })
                .Verifiable();
            _options = new ExecutionDataflowBlockOptions();
            _loggerMock = new Mock<ILogger>();
            _response = new HttpResponseMessage();
        }

        public static TheoryData<TimeSpan[], Func<HttpRequestMessage, int, Task<HttpResponseMessage>>, int> NotifyAsyncData
        {
            get
            {
                TimeSpan delay = TimeSpan.FromMilliseconds(25);
                return new TheoryData<TimeSpan[], Func<HttpRequestMessage, int, Task<HttpResponseMessage>>, int>
                {
                    { new TimeSpan[0], CreateNotifyResponseHandler(1), 0 },
                    { new[] { delay }, CreateNotifyResponseHandler(2), 1 },
                    { new[] { delay, delay }, CreateNotifyResponseHandler(3), 2 },
                    { new[] { delay, delay, delay }, CreateNotifyResponseHandler(4), 3 },
                    { new[] { delay, delay, delay, delay }, CreateNotifyResponseHandler(5), 4 },

                    { new[] { delay }, CreateNotifyResponseHandler(2, isGone: true), -1 },
                    { new[] { delay, delay }, CreateNotifyResponseHandler(3, isGone: true), -2 },
                    { new[] { delay, delay, delay }, CreateNotifyResponseHandler(4, isGone: true), -3 },
                    { new[] { delay, delay, delay, delay }, CreateNotifyResponseHandler(5, isGone: true), -4 },

                    { new TimeSpan[0], CreateNotifyResponseHandler(1, throwExceptions: true), 0 },
                    { new[] { delay }, CreateNotifyResponseHandler(2, throwExceptions: true), 1 },
                    { new[] { delay, delay }, CreateNotifyResponseHandler(3, throwExceptions: true), 2 },
                    { new[] { delay, delay, delay }, CreateNotifyResponseHandler(4, throwExceptions: true), 3 },
                    { new[] { delay, delay, delay, delay }, CreateNotifyResponseHandler(5, throwExceptions: true), 4 },

                    { new TimeSpan[0], CreateNotifyResponseHandler(1, failuresOnly: true), -1 },
                    { new[] { delay }, CreateNotifyResponseHandler(2, failuresOnly: true), -2 },
                    { new[] { delay, delay }, CreateNotifyResponseHandler(3, failuresOnly: true), -3 },
                    { new[] { delay, delay, delay }, CreateNotifyResponseHandler(4, failuresOnly: true), -4 },
                    { new[] { delay, delay, delay, delay }, CreateNotifyResponseHandler(5, failuresOnly: true), -5 },

                    { new TimeSpan[0], CreateNotifyResponseHandler(1, failuresOnly: true, throwExceptions: true), -1 },
                    { new[] { delay }, CreateNotifyResponseHandler(2, failuresOnly: true, throwExceptions: true), -2 },
                    { new[] { delay, delay }, CreateNotifyResponseHandler(3, failuresOnly: true, throwExceptions: true), -3 },
                    { new[] { delay, delay, delay }, CreateNotifyResponseHandler(4, failuresOnly: true, throwExceptions: true), -4 },
                    { new[] { delay, delay, delay, delay }, CreateNotifyResponseHandler(5, failuresOnly: true, throwExceptions: true), -5 },
                };
            }
        }

        public static TheoryData<IEnumerable<WebHook>, NotificationDictionary> FilterSingleNotificationData
        {
            get
            {
                return new TheoryData<IEnumerable<WebHook>, NotificationDictionary>
                {
                    { new[] { CreateWebHook("_") }, CreateNotification("a") },
                    { new[] { CreateWebHook("a") }, CreateNotification("a") },
                    { new[] { CreateWebHook("*") }, CreateNotification("a") },

                    { new[] { CreateWebHook("_"), CreateWebHook("a") }, CreateNotification("a") },
                    { new[] { CreateWebHook("a"), CreateWebHook("_") }, CreateNotification("a") },
                    { new[] { CreateWebHook("*"), CreateWebHook("_") }, CreateNotification("a") },
                    { new[] { CreateWebHook("*"), CreateWebHook("*") }, CreateNotification("a") },
                };
            }
        }

        public static TheoryData<IEnumerable<WebHook>, IEnumerable<NotificationDictionary>, int> FilterMultipleNotificationData
        {
            get
            {
                return new TheoryData<IEnumerable<WebHook>, IEnumerable<NotificationDictionary>, int>
                {
                    { new WebHook[0], new NotificationDictionary[0], 0 },

                    { new[] { CreateWebHook("_") }, new[] { CreateNotification("a"), CreateNotification("b"), CreateNotification("c") }, 0 },
                    { new[] { CreateWebHook("a") }, new[] { CreateNotification("a"), CreateNotification("b"), CreateNotification("c") }, 1 },
                    { new[] { CreateWebHook("b") }, new[] { CreateNotification("a"), CreateNotification("b"), CreateNotification("c") }, 1 },
                    { new[] { CreateWebHook("c") }, new[] { CreateNotification("a"), CreateNotification("b"), CreateNotification("c") }, 1 },
                    { new[] { CreateWebHook("*") }, new[] { CreateNotification("a"), CreateNotification("b"), CreateNotification("c") }, 1 },

                    { new[] { CreateWebHook("_"), CreateWebHook("_"), CreateWebHook("_") }, new[] { CreateNotification("a"), CreateNotification("b"), CreateNotification("c") }, 0 },
                    { new[] { CreateWebHook("_"), CreateWebHook("a"), CreateWebHook("_") }, new[] { CreateNotification("a"), CreateNotification("b"), CreateNotification("c") }, 1 },
                    { new[] { CreateWebHook("a"), CreateWebHook("_"), CreateWebHook("a") }, new[] { CreateNotification("a"), CreateNotification("b"), CreateNotification("c") }, 2 },
                    { new[] { CreateWebHook("a"), CreateWebHook("a"), CreateWebHook("a") }, new[] { CreateNotification("a"), CreateNotification("b"), CreateNotification("c") }, 3 },
                    { new[] { CreateWebHook("*"), CreateWebHook("a"), CreateWebHook("*") }, new[] { CreateNotification("a"), CreateNotification("b"), CreateNotification("c") }, 3 },
                };
            }
        }

        [Fact]
        public async Task VerifyWebHookAsync_Throws_IfHttpClientThrows()
        {
            // Arrange
            _manager = new WebHookManager(_storeMock.Object, _loggerMock.Object, new TimeSpan[0], _options, _httpClient, onWebHookSuccess: null, onWebHookFailure: null);
            _handlerMock.Handler = (req, counter) => { throw new Exception("Catch this!"); };
            WebHook webHook = CreateWebHook();

            // Act
            InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _manager.VerifyWebHookAsync(webHook));

            // Assert
            Assert.Equal("WebHook verification failed. Please ensure that the WebHook URI is valid and that the endpoint is accessible. Error encountered: Catch this!", ex.Message);
        }

        [Fact]
        public async Task VerifyWebHookAsync_Throws_IEmptySuccessResponse()
        {
            // Arrange
            _manager = new WebHookManager(_storeMock.Object, _loggerMock.Object, new TimeSpan[0], _options, _httpClient, onWebHookSuccess: null, onWebHookFailure: null);
            _handlerMock.Handler = (req, counter) => Task.FromResult(_response);
            WebHook webHook = CreateWebHook();

            // Act
            InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _manager.VerifyWebHookAsync(webHook));

            // Assert
            Assert.Equal("The WebHook URI did not return the expected echo query parameter value in a plain text response body. This is necessary to ensure that the WebHook is connected correctly.", ex.Message);
        }

        [Fact]
        public async Task VerifyWebHookAsync_Throws_INotSuccessResponse()
        {
            // Arrange
            _response.StatusCode = HttpStatusCode.NotFound;
            _manager = new WebHookManager(_storeMock.Object, _loggerMock.Object, new TimeSpan[0], _options, _httpClient, onWebHookSuccess: null, onWebHookFailure: null);
            _handlerMock.Handler = (req, counter) => Task.FromResult(_response);
            WebHook webHook = CreateWebHook();

            // Act
            InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _manager.VerifyWebHookAsync(webHook));

            // Assert
            Assert.Equal("WebHook verification failed. Please ensure that the WebHook URI is valid and that the endpoint is accessible. Error encountered: NotFound", ex.Message);
        }

        [Fact]
        public async Task VerifyWebHookAsync_Throws_IfEchoDoesNotMatch()
        {
            // Arrange
            _response.Content = new StringContent("Hello World");
            _manager = new WebHookManager(_storeMock.Object, _loggerMock.Object, new TimeSpan[0], _options, _httpClient, onWebHookSuccess: null, onWebHookFailure: null);
            _handlerMock.Handler = (req, counter) => Task.FromResult(_response);
            WebHook webHook = CreateWebHook();

            // Act
            InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _manager.VerifyWebHookAsync(webHook));

            // Assert
            Assert.Equal("The HTTP request echo query parameter was not returned as plain text in the response. Please return the echo parameter to verify that the WebHook is working as expected.", ex.Message);
        }

        [Fact]
        public async Task VerifyWebHookAsync_Succeeds_EchoResponse()
        {
            // Arrange
            _manager = new WebHookManager(_storeMock.Object, _loggerMock.Object, new TimeSpan[0], _options, _httpClient, onWebHookSuccess: null, onWebHookFailure: null);
            _handlerMock.Handler = (req, counter) =>
            {
                NameValueCollection query = req.RequestUri.ParseQueryString();
                _response.Content = new StringContent(query["echo"]);
                return Task.FromResult(_response);
            };
            WebHook webHook = CreateWebHook();

            // Act
            await _manager.VerifyWebHookAsync(webHook);
        }

        [Theory]
        [MemberData("NotifyAsyncData")]
        public async Task NotifyAsync_StopsOnLastLastFailureOrFirstSuccessAndFirstGone(TimeSpan[] delays, Func<HttpRequestMessage, int, Task<HttpResponseMessage>> handler, int expected)
        {
            // Arrange
            ManualResetEvent done = new ManualResetEvent(initialState: false);
            WebHookWorkItem success = null, failure = null;
            _manager = new WebHookManager(_storeMock.Object, _loggerMock.Object, delays, _options, _httpClient, onWebHookSuccess: item =>
            {
                success = item;
                done.Set();
            }, onWebHookFailure: item =>
            {
                failure = item;
                done.Set();
            });
            _handlerMock.Handler = handler;
            NotificationDictionary notification = new NotificationDictionary("a1", data: null);

            // Act
            int actual = await _manager.NotifyAsync(TestUser, new[] { notification });
            done.WaitOne();

            // Assert
            _storeMock.Verify();
            if (expected >= 0)
            {
                Assert.Equal(expected, success.Offset);
            }
            else
            {
                Assert.Equal(Math.Abs(expected), failure.Offset);
            }
        }

        [Theory]
        [MemberData("FilterSingleNotificationData")]
        public void GetWorkItems_FilterSingleNotification(IEnumerable<WebHook> webHooks, NotificationDictionary notification)
        {
            // Act
            IEnumerable<WebHookWorkItem> actual = WebHookManager.GetWorkItems(webHooks.ToArray(), new[] { notification });

            // Assert
            Assert.Equal(webHooks.Count(), actual.Count());
            foreach (WebHookWorkItem workItem in actual)
            {
                Assert.Same(workItem.Notifications.Single(), notification);
            }
        }

        [Theory]
        [MemberData("FilterMultipleNotificationData")]
        public void GetWorkItems_FilterMultipleNotifications(IEnumerable<WebHook> webHooks, IEnumerable<NotificationDictionary> notifications, int expected)
        {
            // Act
            IEnumerable<WebHookWorkItem> actual = WebHookManager.GetWorkItems(webHooks.ToArray(), notifications.ToArray());

            // Assert
            Assert.Equal(expected, actual.Count());
            foreach (WebHookWorkItem workItem in actual)
            {
                foreach (NotificationDictionary notification in workItem.Notifications)
                {
                    Assert.True(workItem.WebHook.MatchesAction(notification.Action));
                }
            }
        }

        [Fact]
        public void CreateWebHookRequest_CreatesExpectedRequest()
        {
            // Arrange
            WebHookWorkItem workItem = CreateWorkItem();
            workItem.WebHook.Headers.Add("Content-Language", "da");

            // Act
            HttpRequestMessage actual = WebHookManager.CreateWebHookRequest(workItem, _loggerMock.Object);

            // Assert
            Assert.Equal(HttpMethod.Post, actual.Method);
            Assert.Equal(workItem.WebHook.WebHookUri, actual.RequestUri.AbsoluteUri);

            IEnumerable<string> headers;
            actual.Headers.TryGetValues("h1", out headers);
            Assert.Equal("hv1", headers.Single());

            actual.Headers.TryGetValues("ms-signature", out headers);
            Assert.Equal(WebHookSignature, headers.Single());

            Assert.Equal("da", actual.Content.Headers.ContentLanguage.Single());
            Assert.Equal("application/json; charset=utf-8", actual.Content.Headers.ContentType.ToString());
        }

        [Fact]
        public void CreateWebHookRequestBody_CreatesExpectedBody()
        {
            // Arrange
            WebHookWorkItem workItem = CreateWorkItem();

            // Act
            JObject actual = WebHookManager.CreateWebHookRequestBody(workItem);

            // Assert
            Assert.Equal(SerializedWebHook, actual.ToString());
        }

        [Fact]
        public async Task SignWebHookRequest_SignsBodyCorrectly()
        {
            // Arrange
            WebHookWorkItem workItem = CreateWorkItem();
            HttpRequestMessage request = new HttpRequestMessage();
            JObject body = WebHookManager.CreateWebHookRequestBody(workItem);

            // Act
            WebHookManager.SignWebHookRequest(workItem.WebHook, request, body);

            // Assert
            IEnumerable<string> signature;
            request.Headers.TryGetValues("ms-signature", out signature);
            Assert.Equal(WebHookSignature, signature.Single());

            string requestBody = await request.Content.ReadAsStringAsync();
            Assert.Equal(SerializedWebHook, requestBody);

            Assert.Equal("application/json; charset=utf-8", request.Content.Headers.ContentType.ToString());
        }

        [Fact]
        public void Dispose_Succeeds()
        {
            // Arrange
            WebHookManager m = new WebHookManager(_storeMock.Object, _loggerMock.Object);

            // Act
            m.Dispose();
            m.Dispose();
        }

        public void Dispose()
        {
            if (_manager != null)
            {
                _manager.Dispose();
            }
            if (_handlerMock != null)
            {
                _handlerMock.Dispose();
            }
            if (_httpClient != null)
            {
                _httpClient.Dispose();
            }
            if (_response != null)
            {
                _response.Dispose();
            }
        }

        private static WebHook CreateWebHook()
        {
            return CreateWebHook("a1");
        }

        private static WebHook CreateWebHook(params string[] filters)
        {
            WebHook hook = new WebHook
            {
                Id = "1234",
                Description = "你好世界",
                Secret = "123456789012345678901234567890123456789012345678",
                WebHookUri = "http://localhost/hook"
            };
            hook.Headers.Add("h1", "hv1");
            hook.Properties.Add("p1", "pv1");
            foreach (string filter in filters)
            {
                hook.Filters.Add(filter);
            }
            return hook;
        }

        private static WebHookWorkItem CreateWorkItem()
        {
            WebHook webHook = CreateWebHook();
            NotificationDictionary notification1 = new NotificationDictionary("a1", new { d1 = "dv1" });
            NotificationDictionary notification2 = new NotificationDictionary("a1", new Dictionary<string, object> { { "d2", new Uri("http://localhost") } });
            WebHookWorkItem workItem = new WebHookWorkItem(webHook, new[] { notification1, notification2 })
            {
                Id = "1234567890",
            };
            return workItem;
        }

        private static NotificationDictionary CreateNotification(string action)
        {
            return new NotificationDictionary(action, data: null);
        }

        private static HttpResponseMessage[] CreateResponseArray(int length, bool failuresOnly = false, bool isGone = false)
        {
            HttpResponseMessage[] responses = new HttpResponseMessage[length];
            int cnt = 0;
            for (; cnt < length - 1; cnt++)
            {
                responses[cnt] = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }

            // Set the final response to be either success or failure.
            responses[cnt] = new HttpResponseMessage(failuresOnly ? HttpStatusCode.InternalServerError : isGone ? HttpStatusCode.Gone : HttpStatusCode.OK);
            return responses;
        }

        private static Func<HttpRequestMessage, int, Task<HttpResponseMessage>> CreateNotifyResponseHandler(int requests, bool failuresOnly = false, bool throwExceptions = false, bool isGone = false)
        {
            HttpResponseMessage[] responses = CreateResponseArray(requests, failuresOnly, isGone);
            return (req, counter) =>
            {
                HttpResponseMessage response = responses[counter];
                if (throwExceptions && !response.IsSuccessStatusCode)
                {
                    throw new Exception("Catch this!");
                }
                return Task.FromResult(response);
            };
        }
    }
}
