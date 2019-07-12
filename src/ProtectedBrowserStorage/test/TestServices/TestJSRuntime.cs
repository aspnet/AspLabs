// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Microsoft.AspNetCore.ProtectedBrowserStorage.Tests.TestServices
{
    public class TestJSRuntime : IJSRuntime
    {
        public List<(string Identifier, object[] Args)> Invocations { get; }
            = new List<(string Identifier, object[] Args)>();

        public object NextInvocationResult { get; set; }

        public Task<TValue> InvokeAsync<TValue>(string identifier, params object[] args)
        {
            Invocations.Add((identifier, args));
            return (Task<TValue>)NextInvocationResult;
        }
    }
}
