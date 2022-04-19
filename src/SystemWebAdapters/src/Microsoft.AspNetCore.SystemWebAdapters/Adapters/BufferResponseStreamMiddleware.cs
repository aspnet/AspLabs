// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal class BufferResponseStreamMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<BufferResponseStreamMiddleware> _logger;

    public BufferResponseStreamMiddleware(RequestDelegate next, ILogger<BufferResponseStreamMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public Task InvokeAsync(HttpContextCore context)
        => context.GetEndpoint()?.Metadata.GetMetadata<IBufferResponseStreamMetadata>() is { IsEnabled: true } metadata && context.Features.Get<IHttpResponseBodyFeature>() is { } feature
            ? BufferResponseStreamAsync(context, feature, metadata)
            : _next(context);

    private async Task BufferResponseStreamAsync(HttpContextCore context, IHttpResponseBodyFeature feature, IBufferResponseStreamMetadata metadata)
    {
        _logger.LogTrace("Buffering response stream");

        var originalBodyFeature = context.Features.Get<IHttpResponseBodyFeature>();
        var originalBufferedResponseFeature = context.Features.Get<IBufferedResponseFeature>();

        var bufferedFeature = new BufferedHttpResponseFeature(feature, metadata);

        context.Features.Set<IHttpResponseBodyFeature>(bufferedFeature);
        context.Features.Set<IBufferedResponseFeature>(bufferedFeature);

        try
        {
            await _next(context);
            await bufferedFeature.FlushBufferedStreamAsync();
        }
        finally
        {
            context.Features.Set(originalBodyFeature);
            context.Features.Set(originalBufferedResponseFeature);
        }
    }
}
