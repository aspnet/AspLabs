// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.AspNet.WebHooks.Mocks;
using Microsoft.AspNet.WebHooks.Storage;
using Microsoft.TestUtilities.Mocks;
using Microsoft.WindowsAzure.Storage.Queue;
using Moq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class AzureWebHookDequeueManagerTests : IDisposable
    {
        private const int MaxAttempts = 0;
        private const string ConnectionString = "UseDevelopmentStorage=true;";

        private TimeSpan _pollingFrequency = TimeSpan.FromMilliseconds(10);
        private TimeSpan _messageTimeout = TimeSpan.FromMilliseconds(10);
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        private ILogger _logger;
        private Mock<IStorageManager> _storageMock;
        private Mock<WebHookSender> _senderMock;
        private AzureWebHookDequeueManagerMock _dequeueManager;
        private HttpMessageHandlerMock _handlerMock;

        public AzureWebHookDequeueManagerTests()
        {
            _logger = new Mock<ILogger>().Object;
            _storageMock = StorageManagerMock.Create();
            _senderMock = new Mock<WebHookSender>(_logger);
            _handlerMock = new HttpMessageHandlerMock();
        }

        public static TheoryData<int[]> DequeueData
        {
            get
            {
                return new TheoryData<int[]>
                {
                    new[] { 0 },
                    new[] { 0, -1, 0, -1, 0 },
                    new[] { 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100 },
                    new[] { 0, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 100 },
                    new[] { 0, 1, 4, -1, 8, 36, 0, -1, 0, 1, 31, 64, 100, 0, 1 },
                };
            }
        }

        public static TheoryData<int[]> SenderData
        {
            get
            {
                return new TheoryData<int[]>
                {
                    new[] { 200, 200, 200 },
                    new[] { -1, -1, -1 },
                    new[] { 400, 500, 404, 401, 503, 302 },
                    new[] { 200, 201, 404, 500, 202, -1, -1 },
                    new[] { -1, 404, 200, 410, 200, 410, -1 },
                };
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(100)]
        [InlineData(250)]
        public async Task Dispose_CancelsStartTask(int millisecondDelay)
        {
            // Arrange
            _dequeueManager = new AzureWebHookDequeueManagerMock(this);

            // Act
            Task actual = _dequeueManager.Start(_tokenSource.Token);
            await Task.Delay(millisecondDelay);
            _dequeueManager.Dispose();
            await actual;

            // Assert
            Assert.True(actual.IsCompleted);
        }

        [Fact]
        public async Task Start_Throws_IfCalledMoreThanOnce()
        {
            // Arrange
            _dequeueManager = new AzureWebHookDequeueManagerMock(this);
            Task start = _dequeueManager.Start(_tokenSource.Token);

            // Act
            InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _dequeueManager.Start(_tokenSource.Token));

            // Assert
            Assert.Contains("This 'AzureWebHookDequeueManagerMock' instance has already been started. It can only be started once.", ex.Message);
        }

        [Theory]
        [MemberData(nameof(DequeueData))]
        public async Task DequeueAndSendWebHooks_GetsMessagesAndSubmitsToSender(int[] count)
        {
            // Arrange
            int index = 0;
            _storageMock.Setup(s => s.GetMessagesAsync(StorageManagerMock.CloudQueue, AzureWebHookDequeueManager.MaxDequeuedMessages, _messageTimeout))
                .Returns(() =>
                {
                    int cnt = index > count.Length ? 0 : count[index++];
                    if (cnt < 0)
                    {
                        throw new Exception("Catch this!");
                    }
                    var result = StorageManagerMock.CreateQueueMessages(cnt);
                    return Task.FromResult(result);
                })
                .Callback(() =>
                {
                    if (index > count.Length)
                    {
                        _tokenSource.Cancel();
                    }
                })
                .Verifiable();
            _dequeueManager = new AzureWebHookDequeueManagerMock(this, storageManager: _storageMock.Object, sender: _senderMock.Object);

            // Act
            await _dequeueManager.DequeueAndSendWebHooks(_tokenSource.Token);

            // Assert
            int expected = count.Where(i => i > 0).Count();
            _senderMock.Verify(s => s.SendWebHookWorkItemsAsync(It.IsAny<IEnumerable<WebHookWorkItem>>()), Times.Exactly(expected));
        }

        [Theory]
        [MemberData(nameof(SenderData))]
        public async Task QueuedSender_Deletes_AllCompletedResponses(int[] statusCodes)
        {
            // Arrange
            _handlerMock.Handler = (req, index) =>
            {
                if (statusCodes[index] < 0)
                {
                    throw new Exception("Catch this!");
                }
                HttpResponseMessage response = req.CreateResponse((HttpStatusCode)statusCodes[index]);
                return Task.FromResult(response);
            };
            HttpClient client = new HttpClient(_handlerMock);
            _dequeueManager = new AzureWebHookDequeueManagerMock(this, storageManager: _storageMock.Object, httpClient: client);
            IEnumerable<WebHookWorkItem> workItems = StorageManagerMock.CreateWorkItems(statusCodes.Length);

            // Act
            await _dequeueManager.WebHookSender.SendWebHookWorkItemsAsync(workItems);

            // Assert
            _storageMock.Verify(s => s.DeleteMessagesAsync(StorageManagerMock.CloudQueue, It.Is<IEnumerable<CloudQueueMessage>>(m => m.Count() == statusCodes.Length)), Times.Once());
        }

        [Theory]
        [MemberData(nameof(SenderData))]
        public async Task QueuedSender_Deletes_SuccessAndGoneResponses(int[] statusCodes)
        {
            // Arrange
            _handlerMock.Handler = (req, index) =>
            {
                if (statusCodes[index] < 0)
                {
                    throw new Exception("Catch this!");
                }
                HttpResponseMessage response = req.CreateResponse((HttpStatusCode)statusCodes[index]);
                return Task.FromResult(response);
            };
            HttpClient client = new HttpClient(_handlerMock);
            _dequeueManager = new AzureWebHookDequeueManagerMock(this, maxAttempts: 1, storageManager: _storageMock.Object, httpClient: client);
            IEnumerable<WebHookWorkItem> workItems = StorageManagerMock.CreateWorkItems(statusCodes.Length);

            // Act
            await _dequeueManager.WebHookSender.SendWebHookWorkItemsAsync(workItems);

            // Assert
            int expected = statusCodes.Where(i => (i >= 200 && i <= 299) || i == 410).Count();
            _storageMock.Verify(s => s.DeleteMessagesAsync(StorageManagerMock.CloudQueue, It.Is<IEnumerable<CloudQueueMessage>>(m => m.Count() == expected)), Times.Once());
        }

        public void Dispose()
        {
            if (_dequeueManager != null)
            {
                _dequeueManager.Dispose();
            }
            if (_handlerMock != null)
            {
                _handlerMock.Dispose();
            }
            if (_tokenSource != null)
            {
                _tokenSource.Dispose();
            }
        }

        private class AzureWebHookDequeueManagerMock : AzureWebHookDequeueManager
        {
            public AzureWebHookDequeueManagerMock(AzureWebHookDequeueManagerTests parent, int maxAttempts = MaxAttempts, HttpClient httpClient = null, IStorageManager storageManager = null, WebHookSender sender = null)
                : base(ConnectionString, parent._logger, parent._pollingFrequency, parent._messageTimeout, maxAttempts, httpClient, storageManager, sender)
            {
            }

            public new Task DequeueAndSendWebHooks(CancellationToken cancellationToken)
            {
                return base.DequeueAndSendWebHooks(cancellationToken);
            }
        }
    }
}
