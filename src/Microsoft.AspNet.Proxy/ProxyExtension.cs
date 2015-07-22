// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Proxy;

namespace Microsoft.AspNet.Builder
{
    public static class ProxyExtension
    {
        /// <summary>
        /// Sends request to remote server as specified in options
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options">Options for setting port, host, and scheme</param>
        /// <returns></returns>
        public static IApplicationBuilder RunProxy(this IApplicationBuilder builder, ProxyOptions options)
        {
            return builder.UseMiddleware<ProxyMiddleware>(options);
        }
    }
}
