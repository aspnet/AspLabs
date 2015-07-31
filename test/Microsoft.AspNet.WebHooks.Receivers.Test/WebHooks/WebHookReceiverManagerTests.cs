// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Moq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class WebHookReceiverManagerTests
    {
        private const string MockReceiverName = "MockReceiver";

        private List<IWebHookReceiver> _receivers;
        private IWebHookReceiver _receiver;
        private Mock<ILogger> _loggerMock;

        public WebHookReceiverManagerTests()
        {
            _receiver = new MockReceiver();
            _receivers = new List<IWebHookReceiver>();
            _loggerMock = new Mock<ILogger>();
        }

        [Theory]
        [InlineData("MockReceiver")]
        [InlineData("MOCKRECEIVER")]
        [InlineData("mockreceiver")]
        public void GetReceiver_Returns_ReceiverWithSingleName(string receiverName)
        {
            // Arrange
            _receivers.Add(_receiver);
            IWebHookReceiverManager manager = new WebHookReceiverManager(_receivers, _loggerMock.Object);

            // Act
            IWebHookReceiver actual = manager.GetReceiver(receiverName);

            // Assert
            Assert.Same(_receiver, actual);
        }

        [Theory]
        [InlineData("MockReceiver", "你好", "世界")]
        [InlineData("你好", "MOCKRECEIVER", "世界")]
        [InlineData("你好", "世界", "mockreceiver")]
        public void GetReceiver_Returns_ReceiverWithMultipleNames(string name1, string name2, string name3)
        {
            // Arrange
            MockReceiver multiReceiver = new MockReceiver(name1, name2, name3);
            _receivers.Add(multiReceiver);
            IWebHookReceiverManager manager = new WebHookReceiverManager(_receivers, _loggerMock.Object);

            // Act
            IWebHookReceiver actual = manager.GetReceiver(MockReceiverName);

            // Assert
            Assert.Same(multiReceiver, actual);
        }

        [Fact]
        public void GetReceiver_Returns_Null_IfNoReceivers()
        {
            // Arrange
            IWebHookReceiverManager manager = new WebHookReceiverManager(_receivers, _loggerMock.Object);

            // Act
            IWebHookReceiver actual = manager.GetReceiver(MockReceiverName);

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public void GetReceiver_Returns_Null_IfUnknownReceiver()
        {
            // Arrange
            MockReceiver multiReceiver = new MockReceiver("你好", "世界", "unknown");
            _receivers.Add(multiReceiver);
            IWebHookReceiverManager manager = new WebHookReceiverManager(_receivers, _loggerMock.Object);

            // Act
            IWebHookReceiver actual = manager.GetReceiver(MockReceiverName);

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public void GetReceiver_Throws_IfDuplicateSingleNameReceivers()
        {
            // Arrange
            _receivers.Add(_receiver);
            _receivers.Add(_receiver);
            _receivers.Add(_receiver);
            IWebHookReceiverManager manager = new WebHookReceiverManager(_receivers, _loggerMock.Object);

            // Act
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => manager.GetReceiver(MockReceiverName));

            // Assert
            Assert.Contains("Multiple types were found that match the WebHook receiver named 'MockReceiver'. This can happen if multiple receivers are defined with the same name but different casing which is not supported. The request for 'MockReceiver' has found the following matching receivers:", ex.Message);
        }

        [Fact]
        public void GetReceiver_Throws_IfDuplicateMultipleNameReceivers()
        {
            // Arrange
            MockReceiver multiReceiver = new MockReceiver(MockReceiverName, MockReceiverName);
            _receivers.Add(multiReceiver);
            IWebHookReceiverManager manager = new WebHookReceiverManager(_receivers, _loggerMock.Object);

            // Act
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => manager.GetReceiver(MockReceiverName));

            // Assert
            Assert.Contains("Multiple types were found that match the WebHook receiver named 'MockReceiver'. This can happen if multiple receivers are defined with the same name but different casing which is not supported. The request for 'MockReceiver' has found the following matching receivers:", ex.Message);
        }

        private class MockReceiver : IWebHookReceiver
        {
            private readonly IEnumerable<string> _names;

            public MockReceiver()
            {
                _names = new string[] { MockReceiverName };
            }

            public MockReceiver(params string[] names)
            {
                _names = names;
            }

            public IEnumerable<string> Names
            {
                get { return _names; }
            }

            public Task<HttpResponseMessage> ReceiveAsync(string receiver, HttpRequestContext context, HttpRequestMessage request)
            {
                throw new NotImplementedException();
            }
        }
    }
}
