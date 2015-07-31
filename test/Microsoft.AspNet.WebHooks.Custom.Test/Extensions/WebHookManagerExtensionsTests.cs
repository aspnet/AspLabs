// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Moq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class WebHookManagerExtensionsTests
    {
        private const string TestUser = "TestUser";

        private string[] _actions = new string[] { "action" };
        private Mock<IWebHookManager> _managerMock;

        public WebHookManagerExtensionsTests()
        {
            _managerMock = new Mock<IWebHookManager>();
        }

        [Fact]
        public void NotifyAsync_PassesThroughDictionaryData()
        {
            // Arrange
            Dictionary<string, object> data = new Dictionary<string, object>();
            _managerMock.Setup(m => m.NotifyAsync(TestUser, _actions, data))
                .ReturnsAsync(0)
                .Verifiable();

            // Act
            WebHookManagerExtensions.NotifyAsync(_managerMock.Object, TestUser, _actions, data);

            // Assert
            _managerMock.Verify();
        }

        [Fact]
        public void NotifyAsync_ConvertsPropertiesToDictionary()
        {
            // Arrange
            IDictionary<string, object> actual = null;
            _managerMock.Setup(m => m.NotifyAsync(TestUser, _actions, It.IsAny<IDictionary<string, object>>()))
                .ReturnsAsync(0)
                .Callback<string, IEnumerable<string>, IDictionary<string, object>>((u, a, d) => { actual = d; })
                .Verifiable();

            // Act
            WebHookManagerExtensions.NotifyAsync(_managerMock.Object, TestUser, _actions, new { k1 = "v", k2 = 1234 });

            // Assert
            _managerMock.Verify();
            Assert.Equal("v", actual["k1"]);
            Assert.Equal(1234, actual["k2"]);
        }

        [Fact]
        public void NotifyAsync_HandlesNullData()
        {
            // Arrange
            _managerMock.Setup(m => m.NotifyAsync(TestUser, _actions, null))
                .ReturnsAsync(0)
                .Verifiable();

            // Act
            WebHookManagerExtensions.NotifyAsync(_managerMock.Object, TestUser, _actions, null);

            // Assert
            _managerMock.Verify();
        }
    }
}
