// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Tools.Trace
{
    public static class CancellationTokenExtensions
    {
        public static Task WaitForCancellationAsync(this CancellationTokenSource cancellationTokenSource) => cancellationTokenSource.Token.WaitForCancellationAsync();
        public static Task WaitForCancellationAsync(this CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<object>();
            cancellationToken.Register(() => tcs.TrySetResult(null));
            return tcs.Task;
        }
    }
}
