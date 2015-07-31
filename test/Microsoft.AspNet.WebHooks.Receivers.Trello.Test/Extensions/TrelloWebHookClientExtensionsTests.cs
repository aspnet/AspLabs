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
    public class TrelloWebHookClientExtensionsTests
    {
        private const string Link = "http://localhost/some/path";
        private const string TestToken = "496dd3ba8b9b42e84a660602a31d683f37f90f049f692a703b07d3e08a9231d5";
        private const string TestAppKey = "7df822c60b2758338b62921c64aded4f";
        private const string TestModelId = "43a28d6a8b46876f44be5589";
        private const string TestDescription = "你好世界";
        private const string TestId = "12b2431e389abdc9c3632516";

        private Mock<UrlHelper> _helperMock;
        private Mock<TrelloWebHookClient> _trelloClientMock;
        private Uri _receiverAddress;

        public TrelloWebHookClientExtensionsTests()
        {
            _helperMock = new Mock<UrlHelper>();
            _trelloClientMock = new Mock<TrelloWebHookClient>(TestToken, TestAppKey);
            _receiverAddress = new Uri(Link);
        }

        [Fact]
        public async Task CreateAsync_CreatesExpectedReceiverUri()
        {
            // Arrange
            _helperMock.Setup(u => u.Link(WebHookReceiverRouteNames.ReceiversAction, It.Is<Dictionary<string, object>>(d => (string)d["webHookReceiver"] == TrelloWebHookReceiver.ReceiverName)))
                .Returns(Link)
                .Verifiable();
            _trelloClientMock.Setup(t => t.CreateAsync(_receiverAddress, TestModelId, TestDescription))
                .ReturnsAsync(TestId)
                .Verifiable();

            // Act
            string actual = await _trelloClientMock.Object.CreateAsync(_helperMock.Object, TestModelId, TestDescription);

            // Assert
            _helperMock.Verify();
            _trelloClientMock.Verify();
            Assert.Equal(TestId, actual);
        }
    }
}
