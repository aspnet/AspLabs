// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http;
using Microsoft.AspNet.WebHooks.Services;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class HttpConfigurationExtensionsTests
    {
        [Fact]
        public void Initialize_SetsStore()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            config.InitializeCustomWebHooksSqlStorage();
            IWebHookStore actual = CustomServices.GetStore();

            // Assert
            Assert.IsType<SqlWebHookStore>(actual);
        }
    }
}
