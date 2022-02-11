// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Extensions;

namespace System.Web
{
    internal class SystemWebStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            => builder =>
            {
                builder.Use(RegisterRawUrl);
                next(builder);
            };

        // RawUrl from System.Web was the original path requested before anything else may have changed it. This method will cache that early on so it can be used if needed.
        private static Task RegisterRawUrl(HttpContextCore context, Func<Task> next)
        {
            context.GetMetadata().RawUrl = Uri.UnescapeDataString(context.Request.GetDisplayUrl());
            return next();
        }
    }
}
