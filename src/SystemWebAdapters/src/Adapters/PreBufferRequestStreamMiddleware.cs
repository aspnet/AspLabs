// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
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
        var feature = context.Features.Get<IHttpMaxRequestBodySizeFeature>();
        var bufferLimit = (feature?.MaxRequestBodySize, metadata.BufferLimit) switch
        {
            (null, long limit) => limit,
            (long limit, null) => limit,
            (long featureLimit, long metadataLimit) => Math.Min(featureLimit, metadataLimit),
            _ => long.MaxValue,
        };

        _logger.LogTrace("Buffering request stream");

        context.Request.EnableBuffering(metadata.BufferThreshold, bufferLimit);

        await context.Request.Body.DrainAsync(context.RequestAborted);
        context.Request.Body.Position = 0;

        await next(context);
    }
}
