// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.TestUtilities.Mocks;
using Moq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class DataflowWebHookSenderTests : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly ExecutionDataflowBlockOptions _options;
        private readonly HttpMessageHandlerMock _handlerMock;
        private readonly Mock<ILogger> _loggerMock;

        private DataflowWebHookSender _sender;

        public DataflowWebHookSenderTests()
        {
            _handlerMock = new HttpMessageHandlerMock();
            _httpClient = new HttpClient(_handlerMock);
            _options = new ExecutionDataflowBlockOptions();
            _loggerMock = new Mock<ILogger>();
        }

        public enum SendResult
        {
            None = 0,
            Success,
            Gone,
            Failure
        }

        public static TheoryData<TimeSpan[], Func<HttpRequestMessage, int, Task<HttpResponseMessage>>, int, SendResult> NotifyAsyncData
        {
            get
            {
                TimeSpan delay = TimeSpan.FromMilliseconds(25);
                return new TheoryData<TimeSpan[], Func<HttpRequestMessage, int, Task<HttpResponseMessage>>, int, SendResult>
                {
                    { new TimeSpan[0], CreateNotifyResponseHandler(1), 0, SendResult.Success },
                    { new[] { delay }, CreateNotifyResponseHandler(2), 1, SendResult.Success },
                    { new[] { delay, delay }, CreateNotifyResponseHandler(3), 2, SendResult.Success },
                    { new[] { delay, delay, delay }, CreateNotifyResponseHandler(4), 3, SendResult.Success },
                    { new[] { delay, delay, delay, delay }, CreateNotifyResponseHandler(5), 4, SendResult.Success },

                    { new[] { delay }, CreateNotifyResponseHandler(2, isGone: true), 1, SendResult.Gone },
                    { new[] { delay, delay }, CreateNotifyResponseHandler(3, isGone: true), 2, SendResult.Gone },
                    { new[] { delay, delay, delay }, CreateNotifyResponseHandler(4, isGone: true), 3, SendResult.Gone },
                    { new[] { delay, delay, delay, delay }, CreateNotifyResponseHandler(5, isGone: true), 4, SendResult.Gone },

                    { new TimeSpan[0], CreateNotifyResponseHandler(1, throwExceptions: true), 0, SendResult.Success },
                    { new[] { delay }, CreateNotifyResponseHandler(2, throwExceptions: true), 1, SendResult.Success },
                    { new[] { delay, delay }, CreateNotifyResponseHandler(3, throwExceptions: true), 2, SendResult.Success },
                    { new[] { delay, delay, delay }, CreateNotifyResponseHandler(4, throwExceptions: true), 3, SendResult.Success },
                    { new[] { delay, delay, delay, delay }, CreateNotifyResponseHandler(5, throwExceptions: true), 4, SendResult.Success },

                    { new TimeSpan[0], CreateNotifyResponseHandler(1, isGone: true), 0, SendResult.Gone },
                    { new[] { delay }, CreateNotifyResponseHandler(2, isGone: true), 1, SendResult.Gone },
                    { new[] { delay, delay }, CreateNotifyResponseHandler(3, isGone: true), 2, SendResult.Gone },
                    { new[] { delay, delay, delay }, CreateNotifyResponseHandler(4, isGone: true), 3, SendResult.Gone },
                    { new[] { delay, delay, delay, delay }, CreateNotifyResponseHandler(5, isGone: true), 4, SendResult.Gone },

                    { new TimeSpan[0], CreateNotifyResponseHandler(1, failuresOnly: true), 1, SendResult.Failure },
                    { new[] { delay }, CreateNotifyResponseHandler(2, failuresOnly: true), 2, SendResult.Failure },
                    { new[] { delay, delay }, CreateNotifyResponseHandler(3, failuresOnly: true), 3, SendResult.Failure },
                    { new[] { delay, delay, delay }, CreateNotifyResponseHandler(4, failuresOnly: true), 4, SendResult.Failure },
                    { new[] { delay, delay, delay, delay }, CreateNotifyResponseHandler(5, failuresOnly: true), 5, SendResult.Failure },

                    { new TimeSpan[0], CreateNotifyResponseHandler(1, failuresOnly: true, throwExceptions: true), 1, SendResult.Failure },
                    { new[] { delay }, CreateNotifyResponseHandler(2, failuresOnly: true, throwExceptions: true), 2, SendResult.Failure },
                    { new[] { delay, delay }, CreateNotifyResponseHandler(3, failuresOnly: true, throwExceptions: true), 3, SendResult.Failure },
                    { new[] { delay, delay, delay }, CreateNotifyResponseHandler(4, failuresOnly: true, throwExceptions: true), 4, SendResult.Failure },
                    { new[] { delay, delay, delay, delay }, CreateNotifyResponseHandler(5, failuresOnly: true, throwExceptions: true), 5, SendResult.Failure },
                };
            }
        }

        [Theory]
        [MemberData(nameof(NotifyAsyncData))]
        public async Task SendWebHook_StopsOnLastLastFailureOrFirstSuccessAndFirstGone(TimeSpan[] delays, Func<HttpRequestMessage, int, Task<HttpResponseMessage>> handler, int expectedOffset, SendResult expectedResult)
        {
            // Arrange
            SendResult actualResult = SendResult.None;
            ManualResetEvent done = new ManualResetEvent(initialState: false);
            WebHookWorkItem final = null;
            int actualRetries = 0;
            _sender = new TestDataflowWebHookSender(_loggerMock.Object, delays, _options, _httpClient,
            onWebHookRetry: item =>
            {
                actualRetries++;
            },
            onWebHookSuccess: item =>
            {
                final = item;
                actualResult = SendResult.Success;
                done.Set();
            },
            onWebHookGone: item =>
            {
                final = item;
                actualResult = SendResult.Gone;
                done.Set();
            },
            onWebHookFailure: item =>
            {
                final = item;
                actualResult = SendResult.Failure;
                done.Set();
            });
            _handlerMock.Handler = handler;
            NotificationDictionary notification = new NotificationDictionary("a1", data: null);
            WebHook webHook = WebHookManagerTests.CreateWebHook();
            WebHookWorkItem workItem = new WebHookWorkItem(webHook, new[] { notification })
            {
                Id = "1234567890",
            };

            // Act
            await _sender.SendWebHookWorkItemsAsync(new[] { workItem });
            done.WaitOne();

            // Assert
            int expectedRetries = expectedResult == SendResult.Failure ? Math.Max(0, expectedOffset - 1) : expectedOffset;
            Assert.Equal(expectedRetries, actualRetries);
            Assert.Equal(expectedResult, actualResult);
            Assert.Equal(expectedOffset, final.Offset);
        }

        [Fact]
        public void Dispose_Succeeds()
        {
            // Arrange
            DataflowWebHookSender s = new DataflowWebHookSender(_loggerMock.Object);

            // Act
            s.Dispose();
            s.Dispose();
        }

        public void Dispose()
        {
            if (_sender != null)
            {
                _sender.Dispose();
            }
            if (_handlerMock != null)
            {
                _handlerMock.Dispose();
            }
            if (_httpClient != null)
            {
                _httpClient.Dispose();
            }
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

        private class TestDataflowWebHookSender : DataflowWebHookSender
        {
            private readonly Action<WebHookWorkItem> _onRetry, _onSuccess, _onGone, _onFailure;

            public TestDataflowWebHookSender(
                ILogger logger,
                IEnumerable<TimeSpan> retryDelays,
                ExecutionDataflowBlockOptions options,
                HttpClient httpClient,
                Action<WebHookWorkItem> onWebHookRetry,
                Action<WebHookWorkItem> onWebHookSuccess,
                Action<WebHookWorkItem> onWebHookGone,
                Action<WebHookWorkItem> onWebHookFailure)
                : base(logger, retryDelays, options, httpClient)
            {
                _onRetry = onWebHookRetry;
                _onSuccess = onWebHookSuccess;
                _onGone = onWebHookGone;
                _onFailure = onWebHookFailure;
            }

            protected override Task OnWebHookRetry(WebHookWorkItem workItem)
            {
                if (_onRetry != null)
                {
                    _onRetry(workItem);
                }
                return Task.FromResult(true);
            }

            protected override Task OnWebHookSuccess(WebHookWorkItem workItem)
            {
                if (_onSuccess != null)
                {
                    _onSuccess(workItem);
                }
                return Task.FromResult(true);
            }

            protected override Task OnWebHookGone(WebHookWorkItem workItem)
            {
                if (_onGone != null)
                {
                    _onGone(workItem);
                }

                return Task.FromResult(true);
            }

            protected override Task OnWebHookFailure(WebHookWorkItem workItem)
            {
                if (_onFailure != null)
                {
                    _onFailure(workItem);
                }
                return Task.FromResult(true);
            }
        }
    }
}
