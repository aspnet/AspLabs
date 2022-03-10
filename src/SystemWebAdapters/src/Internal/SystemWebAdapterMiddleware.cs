// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace System.Web.Internal
{
    internal class SystemWebAdapterMiddleware : IMiddleware
    {
        public Task InvokeAsync(HttpContextCore context, RequestDelegate next)
        {
            var options = context.GetSystemWebMetadata();

            if (options is not null && options.Enabled)
            {
                return SetupSystemWebAdapterAsync(options, context, next);
            }

            return next(context);
        }

        private static async Task SetupSystemWebAdapterAsync(SystemWebAdapterAttribute options, HttpContextCore context, RequestDelegate next)
        {
            await BufferRequestStreamAsync(options, context);

            await next(context);
        }

        /// <summary>
        /// Set up input stream to be fully buffered so calls such as `.Length` work as in ASP.NET Framework
        /// </summary>
        private static async ValueTask BufferRequestStreamAsync(SystemWebAdapterAttribute options, HttpContextCore context)
        {
            if (!options.BufferRequestStream)
            {
                return;
            }

            context.Request.EnableBuffering();

            var bytes = ArrayPool<byte>.Shared.Rent(1024);

            try
            {
                while (await context.Request.Body.ReadAsync(bytes, context.RequestAborted) > 0)
                {
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(bytes);
            }

            context.Request.Body.Position = 0;
        }
    }
}
