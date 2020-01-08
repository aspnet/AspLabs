// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

#nullable disable

namespace Microsoft.AspNetCore.Grpc.HttpApi
{
    internal class HttpApiServerCallContext : ServerCallContext
    {
        protected override string MethodCore { get; }
        protected override string HostCore { get; }
        protected override string PeerCore { get; }
        protected override DateTime DeadlineCore { get; }
        protected override Metadata RequestHeadersCore { get; }
        protected override CancellationToken CancellationTokenCore { get; }
        protected override Metadata ResponseTrailersCore { get; }
        protected override Status StatusCore { get; set; }
        protected override WriteOptions WriteOptionsCore { get; set; }
        protected override AuthContext AuthContextCore { get; }
        protected override IDictionary<object, object> UserStateCore => base.UserStateCore;

        protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions options)
        {
            throw new NotImplementedException();
        }

        protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
        {
            throw new NotImplementedException();
        }
    }
}
