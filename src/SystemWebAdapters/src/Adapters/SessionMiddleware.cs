// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using System.Web.SessionState;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace System.Web.Adapters;

internal class SessionMiddleware : IMiddleware
{
    private readonly ILogger<SessionMiddleware> _logger;

    public SessionMiddleware(ILogger<SessionMiddleware> logger)
    {
        _logger = logger;
    }

    public Task InvokeAsync(HttpContextCore context, RequestDelegate next)
    {
        if (context.GetEndpoint()?.Metadata.GetMetadata<ISessionMetadata>() is { IsEnabled: true } metadata)
        {
            return ManageStateAsync(context, metadata, next);
        }
        else
        {
            return next(context);
        }
    }

    private async Task ManageStateAsync(HttpContextCore context, ISessionMetadata metadata, RequestDelegate next)
    {
        _logger.LogTrace("Initializing session state");

        var manager = context.RequestServices.GetRequiredService<ISessionManager>();

        await using var state = await manager.CreateAsync(context, metadata);

        context.Features.Set(new HttpSessionState(state));

        await next(context);
    }
}
