// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http.Routing;
using Microsoft.AspNet.WebHooks;
using Microsoft.AspNet.WebHooks.Routes;
using Moq;
using Xunit;

namespace System.Web.Http
{
    public class InstagramWebHookClientExtensionsTests
    {
        private const string Link = "http://localhost/some/path";
        private const string TestId = "12b2431e389abdc9c3632516";

        private HttpConfiguration _config;
        private Mock<UrlHelper> _helperMock;
        private Mock<InstagramWebHookClient> _clientMock;
        private Uri _callback;
        private InstagramSubscription _sub;

        public InstagramWebHookClientExtensionsTests()
        {
            _config = new HttpConfiguration();
            _helperMock = new Mock<UrlHelper>();
            _clientMock = new Mock<InstagramWebHookClient>(_config);
            _callback = new Uri(Link);
            _sub = new InstagramSubscription();
        }

        [Fact]
        public void GetCallback_CreatesExpectedReceiverUri()
        {
            // Arrange
            _helperMock.Setup(u => u.Link(WebHookReceiverRouteNames.ReceiversAction, It.Is<Dictionary<string, object>>(d => (string)d["webHookReceiver"] == InstagramWebHookReceiver.ReceiverName && (string)d["id"] == TestId)))
                .Returns(Link)
                .Verifiable();

            // Act
            Uri actual = InstagramWebHookClientExtensions.GetCallback(TestId, _helperMock.Object);

            // Assert
            _helperMock.Verify();
            Assert.Equal(new Uri(Link), actual);
        }

        [Fact]
        public async Task SubscribeAsync_SubscribersUser()
        {
            // Arrange
            _helperMock.Setup(u => u.Link(WebHookReceiverRouteNames.ReceiversAction, It.Is<Dictionary<string, object>>(d => (string)d["webHookReceiver"] == InstagramWebHookReceiver.ReceiverName && (string)d["id"] == TestId)))
                .Returns(Link)
                .Verifiable();
            _clientMock.Setup(c => c.SubscribeAsync(TestId, _callback))
                .ReturnsAsync(_sub)
                .Verifiable();

            // Act
            InstagramSubscription actual = await _clientMock.Object.SubscribeAsync(TestId, _helperMock.Object);

            // Assert
            _helperMock.Verify();
            _clientMock.Verify();
            Assert.Equal(_sub, actual);
        }
    }
}
