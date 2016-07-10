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
    public class VstsWebHookHandlerBaseTests
    {
        private readonly Mock<VstsWebHookHandlerBase> _handlerMock;
        private readonly VstsWebHookHandlerBase _handler;

        private WebHookHandlerContext _context;

        public VstsWebHookHandlerBaseTests()
        {
            _handlerMock = new Mock<VstsWebHookHandlerBase> { CallBase = true };
            _handler = _handlerMock.Object;
        }

        [Fact]
        public void VstsWebHookHandlerBase_SetsReceiverName()
        {
            Assert.Equal(VstsWebHookReceiver.ReceiverName, _handler.Receiver);
        }

        [Fact]
        public async Task ExecuteAsync_Dispatches_BuildCompleted()
        {
            // Arrange
            _context = GetContext("Microsoft.AspNet.WebHooks.Messages.build.complete.json", "build.complete");

            // Act
            await _handler.ExecuteAsync(VstsWebHookReceiver.ReceiverName, _context);

            // Assert
            _handlerMock.Verify(h => h.ExecuteAsync(_context, It.IsAny<BuildCompletedPayload>()), Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_Dispatches_CodeCheckedIn()
        {
            // Arrange
            _context = GetContext("Microsoft.AspNet.WebHooks.Messages.tfvc.checkin.json", "tfvc.checkin");

            // Act
            await _handler.ExecuteAsync(VstsWebHookReceiver.ReceiverName, _context);

            // Assert
            _handlerMock.Verify(h => h.ExecuteAsync(_context, It.IsAny<CodeCheckedInPayload>()), Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_Dispatches_TeamRoomMessagePosted()
        {
            // Arrange
            _context = GetContext("Microsoft.AspNet.WebHooks.Messages.message.posted.json", "message.posted");

            // Act
            await _handler.ExecuteAsync(VstsWebHookReceiver.ReceiverName, _context);

            // Assert
            _handlerMock.Verify(h => h.ExecuteAsync(_context, It.IsAny<TeamRoomMessagePostedPayload>()), Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_Dispatches_WorkItemCommentedOn()
        {
            // Arrange
            _context = GetContext("Microsoft.AspNet.WebHooks.Messages.workitem.commented.json", "workitem.commented");

            // Act
            await _handler.ExecuteAsync(VstsWebHookReceiver.ReceiverName, _context);

            // Assert
            _handlerMock.Verify(h => h.ExecuteAsync(_context, It.IsAny<WorkItemCommentedOnPayload>()), Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_Dispatches_WorkItemCreated()
        {
            // Arrange
            _context = GetContext("Microsoft.AspNet.WebHooks.Messages.workitem.created.json", "workitem.created");

            // Act
            await _handler.ExecuteAsync(VstsWebHookReceiver.ReceiverName, _context);

            // Assert
            _handlerMock.Verify(h => h.ExecuteAsync(_context, It.IsAny<WorkItemCreatedPayload>()), Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_Dispatches_WorkItemDeleted()
        {
            // Arrange
            _context = GetContext("Microsoft.AspNet.WebHooks.Messages.workitem.deleted.json", "workitem.deleted");

            // Act
            await _handler.ExecuteAsync(VstsWebHookReceiver.ReceiverName, _context);

            // Assert
            _handlerMock.Verify(h => h.ExecuteAsync(_context, It.IsAny<WorkItemDeletedPayload>()), Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_Dispatches_WorkItemRestored()
        {
            // Arrange
            _context = GetContext("Microsoft.AspNet.WebHooks.Messages.workitem.restored.json", "workitem.restored");

            // Act
            await _handler.ExecuteAsync(VstsWebHookReceiver.ReceiverName, _context);

            // Assert
            _handlerMock.Verify(h => h.ExecuteAsync(_context, It.IsAny<WorkItemRestoredPayload>()), Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_Dispatches_WorkItemUpdated()
        {
            // Arrange
            _context = GetContext("Microsoft.AspNet.WebHooks.Messages.workitem.updated.json", "workitem.updated");

            // Act
            await _handler.ExecuteAsync(VstsWebHookReceiver.ReceiverName, _context);

            // Assert
            _handlerMock.Verify(h => h.ExecuteAsync(_context, It.IsAny<WorkItemUpdatedPayload>()), Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_Handles_UnknownEventType()
        {
            // Arrange
            _context = GetContext("Microsoft.AspNet.WebHooks.Messages.bad.notMappedEventType.json", "unknown");

            // Act
            await _handler.ExecuteAsync(VstsWebHookReceiver.ReceiverName, _context);

            // Assert
            _handlerMock.Verify(h => h.ExecuteAsync(_context, It.IsAny<JObject>()), Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_Dispatches_GitPush()
        {
            // Arrange
            _context = GetContext("Microsoft.AspNet.WebHooks.Messages.git.push.json", "git.push");

            // Act
            await _handler.ExecuteAsync(VstsWebHookReceiver.ReceiverName, _context);

            // Assert
            _handlerMock.Verify(h => h.ExecuteAsync(_context, It.IsAny<GitPushPayload>()), Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_Dispatches_GitPullRequestCreated()
        {
            // Arrange
            _context = GetContext("Microsoft.AspNet.WebHooks.Messages.git.pullrequest.created.json", "git.pullrequest.created");

            // Act
            await _handler.ExecuteAsync(VstsWebHookReceiver.ReceiverName, _context);

            // Assert
            _handlerMock.Verify(h => h.ExecuteAsync(_context, It.IsAny<GitPullRequestCreatedPayload>()), Times.Once());
        }

        [Fact]
        public async Task ExecuteAsync_Dispatches_GitPullRequestUpdated()
        {
            // Arrange
            _context = GetContext("Microsoft.AspNet.WebHooks.Messages.git.pullrequest.updated.json", "git.pullrequest.created");

            // Act
            await _handler.ExecuteAsync(VstsWebHookReceiver.ReceiverName, _context);

            // Assert
            _handlerMock.Verify(h => h.ExecuteAsync(_context, It.IsAny<GitPullRequestCreatedPayload>()), Times.Once());
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
