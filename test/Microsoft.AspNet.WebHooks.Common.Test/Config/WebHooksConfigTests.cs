// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web.Http;
using Xunit;

namespace Microsoft.AspNet.WebHooks.Config
{
    public class WebHooksConfigTests
    {
        private HttpConfiguration _config;

        public WebHooksConfigTests()
        {
            _config = new HttpConfiguration();
            WebHooksConfig.Reset();
        }

        [Fact]
        public void Initialize_SetsConfig()
        {
            // Act
            WebHooksConfig.Initialize(_config);
            HttpConfiguration actual = WebHooksConfig.Config;

            // Assert
            Assert.Same(_config, actual);
        }

        [Fact]
        public void Config_Throws_IfNotSet()
        {
            // Act
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => WebHooksConfig.Config);

            // Assert
            Assert.StartsWith("WebHooks support has not been initialized correctly. Please call the initializer 'WebHooksConfig.Initialize' on startup.", ex.Message);
        }
    }
}
