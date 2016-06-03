// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.DataProtection;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class DataSecurityTests
    {
        [Fact]
        public void GetDataProtector_ReturnsSingletonInstance()
        {
            // Act
            IDataProtector actual1 = DataSecurity.GetDataProtector();
            IDataProtector actual2 = DataSecurity.GetDataProtector();

            // Assert
            Assert.NotNull(actual1);
            Assert.Same(actual1, actual2);
        }
    }
}
