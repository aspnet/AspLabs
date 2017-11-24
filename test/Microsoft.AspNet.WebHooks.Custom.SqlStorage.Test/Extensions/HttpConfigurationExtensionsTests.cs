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

        [Theory]
        [InlineData("Default")]
        [InlineData("")]
        [InlineData(null)]
        public void Initialize_SetsStore_WithCustomConnectionString(string nameOrConnectionString)
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            config.InitializeCustomWebHooksSqlStorage(true, nameOrConnectionString);
            IWebHookStore actual = CustomServices.GetStore();

            // Assert
            Assert.IsType<SqlWebHookStore>(actual);
        }

        [Theory]
        [InlineData("Default", "dbo", "WebHooksTable")]
        [InlineData("Default", null, null)]
        [InlineData("", "", "")]
        [InlineData(null, null, null)]
        public void Initialize_SetsStore_WithCustomSettings(string nameOrConnectionString, string schemaName, string tableName)
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            config.InitializeCustomWebHooksSqlStorage(true, nameOrConnectionString, schemaName, tableName);
            IWebHookStore actual = CustomServices.GetStore();

            // Assert
            Assert.IsType<SqlWebHookStore>(actual);
        }
    }
}
