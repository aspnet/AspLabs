// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Grpc.Core;
using HttpApi;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Tests.Infrastructure
{
    public class HttpApiGreeterService : HttpApiGreeter.HttpApiGreeterBase
    {
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return base.SayHello(request, context);
        }
    }
}
