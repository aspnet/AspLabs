// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Transport
{
    public abstract class EventPipeClientTransport
    {
        public abstract Task<IDuplexPipe> ConnectAsync(CancellationToken cancellationToken = default);
    }
}
