// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.DataProtection;
using Microsoft.JSInterop;

namespace Microsoft.AspNetCore.ProtectedBrowserStorage
{
    public class ProtectedLocalStorage : ProtectedBrowserStorage
    {
        public ProtectedLocalStorage(IJSRuntime jsRuntime, IDataProtectionProvider dataProtectionProvider)
            : base("localStorage", jsRuntime, dataProtectionProvider)
        {
        }
    }
}
