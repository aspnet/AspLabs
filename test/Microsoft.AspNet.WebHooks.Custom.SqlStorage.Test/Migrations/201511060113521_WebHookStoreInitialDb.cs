// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Entity.Migrations;

namespace Microsoft.AspNet.WebHooks.Migrations
{
    public partial class WebHookStoreInitialDb : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "WebHooks.WebHooks",
                c => new
                {
                    User = c.String(nullable: false, maxLength: 256),
                    Id = c.String(nullable: false, maxLength: 256),
                    ProtectedData = c.String(nullable: false),
                    RowVer = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                })
                .PrimaryKey(t => new { t.User, t.Id });
        }

        public override void Down()
        {
            DropTable("WebHooks.WebHooks");
        }
    }
}
