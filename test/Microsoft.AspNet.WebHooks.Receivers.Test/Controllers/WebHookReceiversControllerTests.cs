// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using Microsoft.AspNet.WebHooks.Mocks;
using Microsoft.TestUtilities.Mocks;
using Moq;
using Xunit;

namespace Microsoft.AspNet.WebHooks.Controllers
{
    public class WebHookReceiversControllerTests
    {
        private const string TestReceiver = "TestReceiver";

        private HttpConfiguration _config;
        private WebHookReceiversController _controller;
        private Mock<IWebHookReceiverManager> _managerMock;

        public WebHookReceiversControllerTests()
        {
            _managerMock = new Mock<IWebHookReceiverManager>();
            _config = HttpConfigurationMock.Create(new[] { new KeyValuePair<Type, object>(typeof(IWebHookReceiverManager), _managerMock.Object) });

            HttpControllerContext controllerContext = new HttpControllerContext()
            {
                Configuration = _config,
                Request = new HttpRequestMessage(),
            };
            _controller = new WebHookReceiversController();
            _controller.ControllerContext = controllerContext;
        }

        [Fact]
        public Task Get_Returns_NotFoundWhenReceiverDoesNotExist()
        {
            return Action_Returns_NotFoundWhenReceiverDoesNotExist(rec => _controller.Get(rec));
        }

        [Fact]
        public Task Get_Returns_ReceiverResponse()
        {
            return Action_Returns_ReceiverResponse(rec => _controller.Get(rec));
        }

        [Fact]
        public Task Get_Handles_ReceiverHttpResponseException()
        {
            return Action_Handles_ReceiverHttpResponseException(rec => _controller.Get(rec));
        }

        [Fact]
        public Task Get_Handles_ReceiverException()
        {
            return Action_Handles_ReceiverException(rec => _controller.Get(rec));
        }

        [Fact]
        public Task Head_Returns_NotFoundWhenReceiverDoesNotExist()
        {
            return Action_Returns_NotFoundWhenReceiverDoesNotExist(rec => _controller.Head(rec));
        }

        [Fact]
        public Task Head_Returns_ReceiverResponse()
        {
            return Action_Returns_ReceiverResponse(rec => _controller.Head(rec));
        }

        [Fact]
        public Task Head_Handles_ReceiverHttpResponseException()
        {
            return Action_Handles_ReceiverHttpResponseException(rec => _controller.Head(rec));
        }

        [Fact]
        public Task Head_Handles_ReceiverException()
        {
            return Action_Handles_ReceiverException(rec => _controller.Head(rec));
        }

        [Fact]
        public Task Post_Returns_NotFoundWhenReceiverDoesNotExist()
        {
            return Action_Returns_NotFoundWhenReceiverDoesNotExist(rec => _controller.Post(rec));
        }

        [Fact]
        public Task Post_Returns_ReceiverResponse()
        {
            return Action_Returns_ReceiverResponse(rec => _controller.Post(rec));
        }

        [Fact]
        public Task Post_Handles_ReceiverHttpResponseException()
        {
            return Action_Handles_ReceiverHttpResponseException(rec => _controller.Post(rec));
        }

        [Fact]
        public Task Post_Handles_ReceiverException()
        {
            return Action_Handles_ReceiverException(rec => _controller.Post(rec));
        }

        private async Task Action_Returns_NotFoundWhenReceiverDoesNotExist(Func<string, Task<IHttpActionResult>> action)
        {
            // Arrange
            _managerMock.Setup(m => m.GetReceiver(TestReceiver))
                .Returns((IWebHookReceiver)null)
                .Verifiable();

            // Act
            IHttpActionResult result = await action(TestReceiver);

            // Assert
            _managerMock.Verify();
            Assert.IsType<NotFoundResult>(result);
        }

        private async Task Action_Returns_ReceiverResponse(Func<string, Task<IHttpActionResult>> action)
        {
            // Arrange
            HttpResponseMessage response = new HttpResponseMessage() { ReasonPhrase = "From Receiver!" };
            WebHookReceiverMock receiver = new WebHookReceiverMock(response);
            _managerMock.Setup(m => m.GetReceiver(TestReceiver))
                .Returns(receiver)
                .Verifiable();

            // Act
            IHttpActionResult result = await action(TestReceiver);
            HttpResponseMessage actual = ((ResponseMessageResult)result).Response;

            // Assert
            _managerMock.Verify();
            Assert.Equal("From Receiver!", actual.ReasonPhrase);
        }

        private async Task Action_Handles_ReceiverHttpResponseException(Func<string, Task<IHttpActionResult>> action)
        {
            // Arrange
            HttpResponseMessage expected = new HttpResponseMessage();
            HttpResponseException exception = new HttpResponseException(expected);
            WebHookReceiverMock receiver = new WebHookReceiverMock(exception);
            _managerMock.Setup(m => m.GetReceiver(TestReceiver))
                .Returns(receiver)
                .Verifiable();

            // Act
            IHttpActionResult result = await action(TestReceiver);
            HttpResponseMessage actual = ((ResponseMessageResult)result).Response;

            // Assert
            _managerMock.Verify();
            Assert.Same(expected, actual);
        }

        private async Task Action_Handles_ReceiverException(Func<string, Task<IHttpActionResult>> action)
        {
            // Arrange
            Exception exception = new Exception("Catch this!");
            WebHookReceiverMock receiver = new WebHookReceiverMock(exception);
            _managerMock.Setup(m => m.GetReceiver(TestReceiver))
                .Returns(receiver)
                .Verifiable();

            // Act
            Exception actual = await Assert.ThrowsAsync<Exception>(() => action(TestReceiver));

            // Assert
            _managerMock.Verify();
            Assert.Equal("Catch this!", actual.Message);
        }
    }
}
