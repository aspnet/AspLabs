// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Grpc.AspNetCore.Server.Model;

namespace IntegrationTestsWebsite.Infrastructure
{
    public class DynamicServiceModelProvider : IServiceMethodProvider<DynamicService>
    {
        public Action<ServiceMethodProviderContext<DynamicService>>? CreateMethod { get; set; }

        public void OnServiceMethodDiscovery(ServiceMethodProviderContext<DynamicService> context)
        {
            Debug.Assert(CreateMethod != null);

            CreateMethod(context);
        }
    }
}
