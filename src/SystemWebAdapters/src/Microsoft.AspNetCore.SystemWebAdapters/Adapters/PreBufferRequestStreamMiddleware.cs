// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal class PreBufferRequestStreamMiddleware
{
    private readonly RequestDelegate _next;

    public PreBufferRequestStreamMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContextCore context)
        => context.GetEndpoint()?.Metadata.GetMetadata<IPreBufferRequestStreamMetadata>() is { IsEnabled: true } metadata
            ? PreBufferAsync(context, metadata)
            : _next(context);

    private async Task PreBufferAsync(HttpContextCore context, IPreBufferRequestStreamMetadata metadata)
    {
        // TODO: Should this enforce MaxRequestBodySize? https://github.com/aspnet/AspLabs/pull/447#discussion_r827314309
#if NET6_0_OR_GREATER
        using (var activity = HttpContextAdapter.Source.StartActivity("PreBufferRequest"))
        {
            activity?.AddTag("BufferThreshold", metadata.BufferThreshold);
            activity?.AddTag("BufferLimit", metadata.BufferLimit);
#else
        {
#endif
        context.Request.EnableBuffering(metadata.BufferThreshold, metadata.BufferLimit ?? long.MaxValue);

            await context.Request.Body.DrainAsync(context.RequestAborted);
            context.Request.Body.Position = 0;
        }

        await _next(context);
    }
}
