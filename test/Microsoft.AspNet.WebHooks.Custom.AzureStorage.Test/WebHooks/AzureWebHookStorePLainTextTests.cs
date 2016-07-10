// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.AspNet.WebHooks.Services;
using Moq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    [Collection("StoreCollection")]
    public class AzureWebHookStorePlaintextTests : WebHookStoreTest
    {
        public AzureWebHookStorePlaintextTests()
            : base(CreateStore())
        {
        }

        [Fact]
        public void CreateStore_Succeeds()
        {
            // Arrange
            ILogger logger = new Mock<ILogger>().Object;

            // Act
            IWebHookStore actual = AzureWebHookStore.CreateStore(logger, encryptData: false);

            // Assert
            Assert.IsType<AzureWebHookStore>(actual);
        }

        private static IWebHookStore CreateStore()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.InitializeCustomWebHooksAzureStorage(encryptData: false);
            IWebHookStore store = CustomServices.GetStore();
            Assert.IsType<AzureWebHookStore>(store);
            return store;
        }
    }
}
