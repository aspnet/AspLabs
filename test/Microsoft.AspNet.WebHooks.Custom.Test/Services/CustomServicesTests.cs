// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Xunit;

namespace Microsoft.AspNet.WebHooks.Services
{
    public class CustomServicesTests
    {
        public CustomServicesTests()
        {
            HttpConfiguration config = new HttpConfiguration();
            WebHooksConfig.Initialize(config);
        }

        [Fact]
        public void GetStore_ReturnsSingleInstance()
        {
            // Act
            IWebHookStore actual1 = CustomServices.GetStore();
            IWebHookStore actual2 = CustomServices.GetStore();

            // Assert
            Assert.Same(actual1, actual2);
        }

        [Fact]
        public void GetFilterProviders_ReturnsSingletonInstance()
        {
            // Act
            IEnumerable<IWebHookFilterProvider> actual1 = CustomServices.GetFilterProviders();
            IEnumerable<IWebHookFilterProvider> actual2 = CustomServices.GetFilterProviders();

            // Assert
            Assert.Same(actual1, actual2);
        }

        [Fact]
        public void GetFilterManager_ReturnsSingletonInstance()
        {
            // Arrange
            List<IWebHookFilterProvider> providers = new List<IWebHookFilterProvider>();

            // Act
            IWebHookFilterManager actual1 = CustomServices.GetFilterManager(providers);
            IWebHookFilterManager actual2 = CustomServices.GetFilterManager(providers);

            // Assert
            Assert.Same(actual1, actual2);
        }

        [Fact]
        public void GetManager_ReturnsSingleInstance()
        {
            // Arrange
            ILogger logger = CommonServices.GetLogger();
            IWebHookStore store = CustomServices.GetStore();

            // Act
            IWebHookManager actual1 = CustomServices.GetManager(store, logger);
            IWebHookManager actual2 = CustomServices.GetManager(store, logger);

            // Assert
            Assert.Same(actual1, actual2);
        }
    }
}
