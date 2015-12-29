// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.AspNet.WebHooks.Services;
using Moq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class HttpConfigurationExtensionsTests
    {
        [Fact]
        public void InitializeSender_SetsSender()
        {
            // Arrange
            ILogger logger = new Mock<ILogger>().Object;
            HttpConfiguration config = new HttpConfiguration();

            // Act
            config.InitializeCustomWebHooksAzureQueueSender();
            IWebHookSender actual = CustomServices.GetSender(logger);

            // Assert
            Assert.IsType<AzureWebHookSender>(actual);
        }

        [Fact]
        public void InitializeStore_SetsStore()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            config.InitializeCustomWebHooksAzureStorage();
            IWebHookStore actual = CustomServices.GetStore();

            // Assert
            Assert.IsType<AzureWebHookStore>(actual);
        }
    }
}
