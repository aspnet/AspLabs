// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using System.Web.Metadata;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace System.Web.Middleware;

internal class BufferRequestStreamMiddleware : IMiddleware
{
    private readonly ILogger<BufferRequestStreamMiddleware> _logger;

    public BufferRequestStreamMiddleware(ILogger<BufferRequestStreamMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContextCore context, RequestDelegate next)
    {
        var metadata = context.GetEndpoint()?.Metadata.GetMetadata<IBufferRequestStreamMetadata>();

        if (metadata is not null && metadata.IsEnabled)
        {
            _logger.LogTrace("Buffering request stream");

            if (metadata.BufferLimit.HasValue)
            {
                context.Request.EnableBuffering(metadata.BufferThreshold, metadata.BufferLimit.Value);
            }
            else
            {
                context.Request.EnableBuffering(metadata.BufferThreshold);
            }

            await context.Request.Body.DrainAsync(context.RequestAborted);
            context.Request.Body.Position = 0;
        }

        await next(context);
    }
}
