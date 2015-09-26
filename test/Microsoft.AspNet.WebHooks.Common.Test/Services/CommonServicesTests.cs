// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.WebHooks.Config;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Xunit;

namespace Microsoft.AspNet.WebHooks.Services
{
    [Collection("ConfigCollection")]
    public class CommonServicesTests
    {
        [Fact]
        public void GetLogger_ReturnsSingletonInstance()
        {
            // Act
            ILogger actual1 = CommonServices.GetLogger();
            ILogger actual2 = CommonServices.GetLogger();

            // Assert
            Assert.Same(actual1, actual2);
        }

        [Fact]
        public void GetSettings_ReturnsSingleInstance()
        {
            // Act
            SettingsDictionary actual1 = CommonServices.GetSettings();
            SettingsDictionary actual2 = CommonServices.GetSettings();

            // Assert
            Assert.Same(actual1, actual2);
        }
    }
}
