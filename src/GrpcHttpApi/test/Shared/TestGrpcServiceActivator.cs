// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Grpc.AspNetCore.Server;

namespace Grpc.Tests.Shared
{
    internal class TestGrpcServiceActivator<TGrpcService> : IGrpcServiceActivator<TGrpcService> where TGrpcService : class, new()
    {
        public GrpcActivatorHandle<TGrpcService> Create(IServiceProvider serviceProvider)
        {
            return new GrpcActivatorHandle<TGrpcService>(new TGrpcService(), false, null);
        }

        public ValueTask ReleaseAsync(GrpcActivatorHandle<TGrpcService> service)
        {
            return default;
        }
    }
}
