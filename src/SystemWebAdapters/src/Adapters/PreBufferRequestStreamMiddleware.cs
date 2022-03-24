// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace System.Web.Adapters;

internal class PreBufferRequestStreamMiddleware : IMiddleware
{
    private readonly ILogger<PreBufferRequestStreamMiddleware> _logger;

    public PreBufferRequestStreamMiddleware(ILogger<PreBufferRequestStreamMiddleware> logger)
    {
        _logger = logger;
    }

    public Task InvokeAsync(HttpContextCore context, RequestDelegate next)
    {
        if (context.GetEndpoint()?.Metadata.GetMetadata<IPreBufferRequestStreamMetadata>() is { IsEnabled: true } metadata)
        {
            return PreBufferAsync(context, metadata, next);
        }

        return next(context);
    }

    private async Task PreBufferAsync(HttpContextCore context, IPreBufferRequestStreamMetadata metadata, RequestDelegate next)
    {
        // TODO: Should this enforce MaxRequestBodySize? https://github.com/aspnet/AspLabs/pull/447#discussion_r827314309
        _logger.LogTrace("Buffering request stream");

        context.Request.EnableBuffering(metadata.BufferThreshold, metadata.BufferLimit ?? long.MaxValue);

        await context.Request.Body.DrainAsync(context.RequestAborted);
        context.Request.Body.Position = 0;

        await next(context);
    }
}
