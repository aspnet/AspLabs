// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Web.Http;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.AspNet.WebHooks.Services;
using Moq;
using Xunit;
using EF = Microsoft.AspNet.WebHooks.Migrations;

namespace Microsoft.AspNet.WebHooks
{
    [Collection("StoreCollection")]
    public class SqlWebHookStoreTests : WebHookStoreTest
    {
        public SqlWebHookStoreTests()
            : base(CreateStore())
        {
        }

        public static TheoryData<ConnectionSettings> ConnectionSettingsData
        {
            get
            {
                return new TheoryData<ConnectionSettings>
                {
                    null,
                    new ConnectionSettings(WebHookStoreContext.ConnectionStringName, string.Empty),
                };
            }
        }

        [Fact]
        public void CreateStore_Succeeds()
        {
            // Arrange
            ILogger logger = new Mock<ILogger>().Object;

            // Act
            IWebHookStore actual = SqlWebHookStore.CreateStore(logger);

            // Assert
            Assert.IsType<SqlWebHookStore>(actual);
        }

        [Theory]
        [MemberData("ConnectionSettingsData")]
        public void CheckSqlStorageConnectionString_Throws_IfNullOrEmptyConnectionString(ConnectionSettings connectionSettings)
        {
            // Arrange
            SettingsDictionary settings = new SettingsDictionary();
            settings.Connections.Add(WebHookStoreContext.ConnectionStringName, connectionSettings);

            // Act
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => SqlWebHookStore.CheckSqlStorageConnectionString(settings));

            // Assert
            Assert.Equal("Please provide an Azure Table Storage connection string with name 'MS_SqlStoreConnectionString' in the configuration string section of the 'Web.Config' file.", ex.Message);
        }

        private static IWebHookStore CreateStore()
        {
            // Delete any existing DB
            string connectionString = ConfigurationManager.ConnectionStrings[WebHookStoreContext.ConnectionStringName].ConnectionString;
            Database.Delete(connectionString);

            // Initialize DB using code first migration
            var dbConfig = new EF.Configuration();
            var migrator = new DbMigrator(dbConfig);
            migrator.Update();

            HttpConfiguration config = new HttpConfiguration();
            config.InitializeCustomWebHooksSqlStorage();
            IWebHookStore store = CustomServices.GetStore();
            Assert.IsType<SqlWebHookStore>(store);
            return store;
        }
    }
}
