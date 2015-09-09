// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class WebHookReceiverConfigExtensionsTests
    {
        private const string Config = "12345678";
        private const string ConfigName = "Name";
        private const string ConfigId = "Id";

        private Mock<IWebHookReceiverConfig> _configMock;

        public WebHookReceiverConfigExtensionsTests()
        {
            _configMock = new Mock<IWebHookReceiverConfig>();
        }

        [Fact]
        public async Task GetReceiverConfigAsync_CallsConfig()
        {
            // Act
            await _configMock.Object.GetReceiverConfigAsync(ConfigName, ConfigId, 0, 1024);

            // Assert
            _configMock.Verify(c => c.GetReceiverConfigAsync(ConfigName, ConfigId), Times.Once());
        }

        [Theory]
        [InlineData(0, 1024, Config)]
        [InlineData(0, 8, Config)]
        [InlineData(16, 1024, null)]
        [InlineData(0, 6, null)]
        public async Task GetReceiverConfigAsync_Checks_Length(int minLength, int maxLength, string expected)
        {
            // Arrange
            _configMock.Setup(c => c.GetReceiverConfigAsync(ConfigName, ConfigId))
                .ReturnsAsync(Config)
                .Verifiable();

            // Act
            string actual = await _configMock.Object.GetReceiverConfigAsync(ConfigName, ConfigId, minLength, maxLength);

            // Assert
            _configMock.Verify();
            Assert.Equal(expected, actual);
        }
    }
}
