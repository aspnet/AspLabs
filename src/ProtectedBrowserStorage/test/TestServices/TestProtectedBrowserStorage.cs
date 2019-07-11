// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.DataProtection;
using Microsoft.JSInterop;

namespace Microsoft.AspNetCore.ProtectedBrowserStorage.Tests.TestServices
{
    public class TestProtectedBrowserStorage : ProtectedBrowserStorage
    {
        public TestProtectedBrowserStorage(string storeName, IJSRuntime jsRuntime, IDataProtectionProvider dataProtectionProvider)
            : base(storeName, jsRuntime, dataProtectionProvider)
        {
        }
    }
}
