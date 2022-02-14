// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Google.Api;
using Google.Protobuf.Reflection;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Internal
{
    internal interface IServiceInvokerResolver<TService> where TService : class
    {
        (TDelegate invoker, List<object> metadata) CreateModelCore<TDelegate>(
            string methodName,
            Type[] methodParameters,
            string verb,
            HttpRule httpRule,
            MethodDescriptor methodDescriptor) where TDelegate : Delegate;
    }
}
