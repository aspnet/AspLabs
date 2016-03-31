// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.WebHooks.Payloads;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks.Handlers
{
    public class MyGetWebHookHandlerBaseTests
    {
        private readonly Mock<MyGetWebHookHandlerBase> _handlerMock;
        private readonly MyGetWebHookHandlerBase _handler;

        private WebHookHandlerContext _context;

        public MyGetWebHookHandlerBaseTests()
        {
            _handlerMock = new Mock<MyGetWebHookHandlerBase> { CallBase = true };
            _handler = _handlerMock.Object;
        }

        [Fact]
        public void MyGetWebHookHandlerBase_SetsReceiverName()
        {
            Assert.Equal(MyGetWebHookReceiver.ReceiverName, _handler.Receiver);
        }

        [Fact]
        public async Task ExecuteAsync_Handles_NoData()
        {
            // Arrange
            _context = GetContext("Microsoft.AspNet.WebHooks.Messages.InvalidMessage.json", "notused");
            _context.Data = null;

            // Act
            await _handler.ExecuteAsync(MyGetWebHookReceiver.ReceiverName, _context);

            // Assert
            HttpError error = await _context.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request must contain a 'Payload' JSON property containing the event payload.", error.Message);
        }

        [Fact]
        public async Task ExecuteAsync_Handles_NoPayloadProperty()
        {
            // Arrange
            _context = GetContext("Microsoft.AspNet.WebHooks.Messages.NoPayloadMessage.json", "None");

            // Act
            await _handler.ExecuteAsync(MyGetWebHookReceiver.ReceiverName, _context);

            // Assert
            HttpError error = await _context.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("The WebHook request must contain a 'Payload' JSON property containing the event payload.", error.Message);
        }

        [Fact]
        public async Task ExecuteAsync_Dispatches_PackageAdded()
        {
            // Arrange
            _context = GetContext("Microsoft.AspNet.WebHooks.Messages.PackageAddedMessage.json", "PackageAddedWebHookEventPayloadV1");

            // Act
            await _handler.ExecuteAsync(MyGetWebHookReceiver.ReceiverName, _context);

            // Assert
            _handlerMock.Verify(h => h.ExecuteAsync(MyGetWebHookReceiver.ReceiverName, _context, It.IsAny<PackageAddedPayload>()), Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_Dispatches_PackageDeleted()
        {
            // Arrange
            _context = GetContext("Microsoft.AspNet.WebHooks.Messages.PackageDeletedMessage.json", "PackageDeletedWebHookEventPayloadV1");

            // Act
            await _handler.ExecuteAsync(MyGetWebHookReceiver.ReceiverName, _context);

            // Assert
            _handlerMock.Verify(h => h.ExecuteAsync(MyGetWebHookReceiver.ReceiverName, _context, It.IsAny<PackageDeletedPayload>()), Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_Dispatches_PackageListed()
        {
            // Arrange
            _context = GetContext("Microsoft.AspNet.WebHooks.Messages.PackageListedMessage.json", "PackageListedWebHookEventPayloadV1");

            // Act
            await _handler.ExecuteAsync(MyGetWebHookReceiver.ReceiverName, _context);

            // Assert
            _handlerMock.Verify(h => h.ExecuteAsync(MyGetWebHookReceiver.ReceiverName, _context, It.IsAny<PackageListedPayload>()), Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_Dispatches_PackagePinned()
        {
            // Arrange
            _context = GetContext("Microsoft.AspNet.WebHooks.Messages.PackagePinnedMessage.json", "PackagePinnedWebHookEventPayloadV1");

            // Act
            await _handler.ExecuteAsync(MyGetWebHookReceiver.ReceiverName, _context);

            // Assert
            _handlerMock.Verify(h => h.ExecuteAsync(MyGetWebHookReceiver.ReceiverName, _context, It.IsAny<PackagePinnedPayload>()), Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_Dispatches_PackagePushed()
        {
            // Arrange
            _context = GetContext("Microsoft.AspNet.WebHooks.Messages.PackageListedMessage.json", "PackageListedWebHookEventPayloadV1");

            // Act
            await _handler.ExecuteAsync(MyGetWebHookReceiver.ReceiverName, _context);

            // Assert
            _handlerMock.Verify(h => h.ExecuteAsync(MyGetWebHookReceiver.ReceiverName, _context, It.IsAny<PackageListedPayload>()), Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_Dispatches_BuildQueued()
        {
            // Arrange
            _context = GetContext("Microsoft.AspNet.WebHooks.Messages.BuildQueuedMessage.json", "BuildQueuedWebHookEventPayloadV1");

            // Act
            await _handler.ExecuteAsync(MyGetWebHookReceiver.ReceiverName, _context);

            // Assert
            _handlerMock.Verify(h => h.ExecuteAsync(MyGetWebHookReceiver.ReceiverName, _context, It.IsAny<BuildQueuedPayload>()), Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_Dispatches_BuildStarted()
        {
            // Arrange
            _context = GetContext("Microsoft.AspNet.WebHooks.Messages.BuildStartedMessage.json", "BuildStartedWebHookEventPayloadV1");

            // Act
            await _handler.ExecuteAsync(MyGetWebHookReceiver.ReceiverName, _context);

            // Assert
            _handlerMock.Verify(h => h.ExecuteAsync(MyGetWebHookReceiver.ReceiverName, _context, It.IsAny<BuildStartedPayload>()), Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_Dispatches_BuildFinished()
        {
            // Arrange
            _context = GetContext("Microsoft.AspNet.WebHooks.Messages.BuildFinishedMessage.json", "BuildFinishedWebHookEventPayloadV1");

            // Act
            await _handler.ExecuteAsync(MyGetWebHookReceiver.ReceiverName, _context);

            // Assert
            _handlerMock.Verify(h => h.ExecuteAsync(MyGetWebHookReceiver.ReceiverName, _context, It.IsAny<BuildFinishedPayload>()), Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_Handles_UnknownPayloadProperty()
        {
            // Arrange
            _context = GetContext("Microsoft.AspNet.WebHooks.Messages.UnknownMessage.json", "Unknown");

            // Act
            await _handler.ExecuteAsync(MyGetWebHookReceiver.ReceiverName, _context);

            // Assert
            _handlerMock.Verify(h => h.ExecuteUnknownPayloadAsync(MyGetWebHookReceiver.ReceiverName, _context, It.IsAny<JObject>()), Times.Once());
        }

        private static WebHookHandlerContext GetContext(string payload, string action)
        {
            JObject data = EmbeddedResource.ReadAsJObject(payload);
            HttpConfiguration httpConfig = new HttpConfiguration();
            HttpRequestContext requestContext = new HttpRequestContext { Configuration = httpConfig };
            HttpRequestMessage request = new HttpRequestMessage();
            request.SetRequestContext(requestContext);
            IEnumerable<string> actions = new[] { action };
            return new WebHookHandlerContext(actions)
            {
                Data = data,
                Request = request,
                RequestContext = requestContext
            };
        }
    }
}
