// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.Results;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.AspNet.WebHooks.Mocks;
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
        private Mock<IDependencyResolver> _resolverMock;

        public WebHookReceiversControllerTests()
        {
            _resolverMock = new Mock<IDependencyResolver>();
            _managerMock = new Mock<IWebHookReceiverManager>();
            _resolverMock.Setup(r => r.GetService(typeof(IWebHookReceiverManager)))
                .Returns(_managerMock.Object)
                .Verifiable();

            _config = new HttpConfiguration();
            _config.DependencyResolver = _resolverMock.Object;
            WebHooksConfig.Initialize(_config);

            HttpControllerContext controllerContext = new HttpControllerContext()
            {
                Configuration = _config,
                Request = new HttpRequestMessage(),
            };
            _controller = new WebHookReceiversController();
            _controller.ControllerContext = controllerContext;
        }

        [Fact]
        public async Task Get_Returns_NotFoundWhenReceiverDoesNotExist()
        {
            // Arrange
            _managerMock.Setup(m => m.GetReceiver(TestReceiver))
                .Returns((IWebHookReceiver)null)
                .Verifiable();

            // Act
            IHttpActionResult result = await _controller.Get(TestReceiver);

            // Assert
            _managerMock.Verify();
            _resolverMock.Verify();
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Get_Returns_ReceiverResponse()
        {
            // Arrange
            HttpResponseMessage response = new HttpResponseMessage() { ReasonPhrase = "From Receiver!" };
            WebHookReceiverMock receiver = new WebHookReceiverMock(response);
            _managerMock.Setup(m => m.GetReceiver(TestReceiver))
                .Returns(receiver)
                .Verifiable();

            // Act
            IHttpActionResult result = await _controller.Get(TestReceiver);
            HttpResponseMessage actual = ((ResponseMessageResult)result).Response;

            // Assert
            _managerMock.Verify();
            _resolverMock.Verify();
            Assert.Equal("From Receiver!", actual.ReasonPhrase);
        }

        [Fact]
        public async Task Get_Handles_ReceiverException()
        {
            // Arrange
            Exception exception = new Exception("Catch this!");
            WebHookReceiverMock receiver = new WebHookReceiverMock(exception);
            _managerMock.Setup(m => m.GetReceiver(TestReceiver))
                .Returns(receiver)
                .Verifiable();

            // Act
            IHttpActionResult result = await _controller.Get(TestReceiver);
            HttpResponseMessage response = ((ResponseMessageResult)result).Response;
            HttpError error = await response.Content.ReadAsAsync<HttpError>();

            // Assert
            _managerMock.Verify();
            _resolverMock.Verify();
            Assert.Equal("WebHook receiver 'TestReceiver' could not process WebHook due to error: Catch this!", error.Message);
        }

        [Fact]
        public async Task Post_Returns_NotFoundWhenReceiverDoesNotExist()
        {
            // Arrange
            _managerMock.Setup(m => m.GetReceiver(TestReceiver))
                .Returns((IWebHookReceiver)null)
                .Verifiable();

            // Act
            IHttpActionResult result = await _controller.Post(TestReceiver);

            // Assert
            _managerMock.Verify();
            _resolverMock.Verify();
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Post_Returns_ReceiverResponse()
        {
            // Arrange
            HttpResponseMessage response = new HttpResponseMessage() { ReasonPhrase = "From Receiver!" };
            WebHookReceiverMock receiver = new WebHookReceiverMock(response);
            _managerMock.Setup(m => m.GetReceiver(TestReceiver))
                .Returns(receiver)
                .Verifiable();

            // Act
            IHttpActionResult result = await _controller.Post(TestReceiver);
            HttpResponseMessage actual = ((ResponseMessageResult)result).Response;

            // Assert
            _managerMock.Verify();
            _resolverMock.Verify();
            Assert.Equal("From Receiver!", actual.ReasonPhrase);
        }

        [Fact]
        public async Task Post_Handles_ReceiverException()
        {
            // Arrange
            Exception exception = new Exception("Catch this!");
            WebHookReceiverMock receiver = new WebHookReceiverMock(exception);
            _managerMock.Setup(m => m.GetReceiver(TestReceiver))
                .Returns(receiver)
                .Verifiable();

            // Act
            IHttpActionResult result = await _controller.Post(TestReceiver);
            HttpResponseMessage response = ((ResponseMessageResult)result).Response;
            HttpError error = await response.Content.ReadAsAsync<HttpError>();

            // Assert
            _managerMock.Verify();
            _resolverMock.Verify();
            Assert.Equal("WebHook receiver 'TestReceiver' could not process WebHook due to error: Catch this!", error.Message);
        }

        [Fact]
        public async Task Post_Handles_ReceiverHttpResponseException()
        {
            // Arrange
            HttpResponseMessage response = new HttpResponseMessage();
            HttpResponseException exception = new HttpResponseException(response);
            WebHookReceiverMock receiver = new WebHookReceiverMock(exception);
            _managerMock.Setup(m => m.GetReceiver(TestReceiver))
                .Returns(receiver)
                .Verifiable();

            // Act
            IHttpActionResult result = await _controller.Post(TestReceiver);
            HttpResponseMessage actual = ((ResponseMessageResult)result).Response;

            // Assert
            _managerMock.Verify();
            _resolverMock.Verify();
            Assert.Same(response, actual);
        }
    }
}
