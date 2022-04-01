// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using BenchmarkDotNet.Configs;

namespace Microsoft.AspNetCore.Grpc.Microbenchmarks
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class DefaultCoreConfigAttribute : Attribute, IConfigSource
    {
        public IConfig Config => new DefaultCoreConfig();
    }
}
