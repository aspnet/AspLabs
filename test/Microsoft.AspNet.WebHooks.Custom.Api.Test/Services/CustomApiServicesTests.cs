// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http;
using Microsoft.AspNet.WebHooks.Config;
using Xunit;

namespace Microsoft.AspNet.WebHooks.Services
{
    [Collection("ConfigCollection")]
    public class CustomApiServicesTests
    {
        public CustomApiServicesTests()
        {
            HttpConfiguration config = new HttpConfiguration();
            WebHooksConfig.Initialize(config);
        }

        [Fact]
        public void GetFilterProviders_ReturnsSingletonInstance()
        {
            // Act
            IEnumerable<IWebHookRegistrar> actual1 = CustomApiServices.GetRegistrars();
            IEnumerable<IWebHookRegistrar> actual2 = CustomApiServices.GetRegistrars();

            // Assert
            Assert.Same(actual1, actual2);
        }
    }
}
