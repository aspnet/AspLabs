// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.DataProtection;
using Microsoft.JSInterop;

namespace Microsoft.AspNetCore.ProtectedBrowserStorage
{
    public class ProtectedSessionStorage : ProtectedBrowserStorage
    {
        public ProtectedSessionStorage(IJSRuntime jsRuntime, IDataProtectionProvider dataProtectionProvider)
            : base("sessionStorage", jsRuntime, dataProtectionProvider)
        {
        }
    }
}
