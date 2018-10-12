// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Transport
{
    public abstract class EventPipeServerTransport
    {
        public abstract void Listen();
        public abstract Task<IDuplexPipe> AcceptAsync(CancellationToken cancellationToken = default);
    }
}
