// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.TestUtilities;
using Moq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class WebHookQueueHandlerTests
    {
        private const string TestReceiver = "TestReceiver";

        private object _data;
        private string[] _actions = new string[] { "a1", "a2" };
        private Mock<WebHookQueueHandler> _queueHandlerMock;
        private WebHookHandlerContext _context;

        public WebHookQueueHandlerTests()
        {
            _queueHandlerMock = new Mock<WebHookQueueHandler>() { CallBase = true };
            _data = new object();
            HttpConfiguration config = new HttpConfiguration();
            HttpRequestContext context = new HttpRequestContext { Configuration = config, };
            HttpRequestMessage request = new HttpRequestMessage();
            request.SetRequestContext(context);
            _context = new WebHookHandlerContext(_actions)
            {
                Data = _data,
                Request = request
            };
        }

        [Fact]
        public void Order_Roundtrips()
        {
            PropertyAssert.Roundtrips(_queueHandlerMock.Object, h => h.Order, defaultValue: WebHookHandler.DefaultOrder, roundtripValue: 100);
        }

        [Fact]
        public void Receiver_Roundtrips()
        {
            PropertyAssert.Roundtrips(_queueHandlerMock.Object, h => h.Receiver, PropertySetter.NullRoundtrips, roundtripValue: "你好世界");
        }

        [Fact]
        public async Task ExecuteAsync_Returns_ErrorResponseOnException()
        {
            // Arrange
            Exception exception = new Exception("Catch this!");
            _queueHandlerMock.Setup(q => q.EnqueueAsync(It.Is<WebHookQueueContext>(c => ValidateContext(c))))
                .Throws(exception)
                .Verifiable();

            // Act
            await _queueHandlerMock.Object.ExecuteAsync(TestReceiver, _context);

            // Assert
            _queueHandlerMock.Verify();
            Assert.Equal(HttpStatusCode.InternalServerError, _context.Response.StatusCode);
            HttpError error = await _context.Response.Content.ReadAsAsync<HttpError>();
            Assert.Equal("Could not enqueue WebHook: Catch this!", error.Message);
        }

        [Fact]
        public async Task ExecuteAsync_Succeeds()
        {
            // Arrange
            _queueHandlerMock.Setup<Task>(q => q.EnqueueAsync(It.Is<WebHookQueueContext>(c => ValidateContext(c))))
                .Returns(Task.FromResult(true))
                .Verifiable();

            // Act
            await _queueHandlerMock.Object.ExecuteAsync(TestReceiver, _context);

            // Assert
            _queueHandlerMock.Verify();
            Assert.Null(_context.Response);
        }

        private bool ValidateContext(WebHookQueueContext context)
        {
            Assert.Same(_data, context.Data);
            Assert.Equal(TestReceiver, context.Receiver);
            Assert.Equal((IEnumerable<string>)_actions, (IEnumerable<string>)context.Actions);
            return true;
        }
    }
}
