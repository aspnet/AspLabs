// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Web.Http;
using Microsoft.AspNet.WebHooks.Services;
using Microsoft.AspNet.WebHooks.Storage;
using Xunit;
using EF = Microsoft.AspNet.WebHooks.Custom.SqlStorage.Migrations;

namespace Microsoft.AspNet.WebHooks
{
    public class SqlWebHookStoreTests : WebHookStoreTest
    {
        public SqlWebHookStoreTests()
            : base(CreateStore())
        {
        }

        private static IWebHookStore CreateStore()
        {
            // Delete any existing DB
            string connectionString = ConfigurationManager.ConnectionStrings[WebHookContext.ConnectionStringName].ConnectionString;
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
