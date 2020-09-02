// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.DynamicJS
{
    internal static class DynamicJSInterop
    {
        private const string JSFunctionsPrefix = "dynamicJS.";

        public const string Evaluate = JSFunctionsPrefix + "evaluate";
    }
}
