// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information

using System.Data.Entity.Migrations;
using Microsoft.AspNet.WebHooks.Storage;

namespace Microsoft.AspNet.WebHooks.Custom.SqlStorage.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<WebHookContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(WebHookContext context)
        {
        }
    }
}
