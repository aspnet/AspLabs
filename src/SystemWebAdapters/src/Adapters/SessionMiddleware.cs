// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using System.Web.SessionState;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace System.Web.Adapters;

internal class SessionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SessionMiddleware> _logger;

    public SessionMiddleware(RequestDelegate next, ILogger<SessionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public Task InvokeAsync(HttpContextCore context)
        => context.GetEndpoint()?.Metadata.GetMetadata<ISessionMetadata>() is { Behavior: not SessionBehavior.None } metadata
            ? ManageStateAsync(context, metadata)
            : _next(context);

    private async Task ManageStateAsync(HttpContextCore context, ISessionMetadata metadata)
    {
        _logger.LogTrace("Initializing session state");

        var manager = context.RequestServices.GetRequiredService<ISessionManager>();

#pragma warning disable CS0618 // Type or member is obsolete
        using var state = metadata.Behavior switch
        {
            SessionBehavior.PreLoad => await manager.CreateAsync(context, metadata),
            SessionBehavior.OnDemand => new LazySessionState(context, _logger, metadata, manager),
            var behavior => throw new InvalidOperationException($"Unknown session behavior {behavior}"),
        };
#pragma warning restore CS0618 // Type or member is obsolete

        context.Features.Set(new HttpSessionState(state));

        try
        {
            await _next(context);
            await state.CommitAsync(context.RequestAborted);
        }
        finally
        {
            context.Features.Set<HttpSessionState?>(null);
        }
    }

    private class LazySessionState : DelegatingSessionState
    {
        private readonly Lazy<ISessionState> _state;

        public LazySessionState(HttpContextCore context, ILogger logger, ISessionMetadata metadata, ISessionManager manager)
        {
            _state = new Lazy<ISessionState>(() =>
            {
                logger.LogWarning("Creating session on demand by synchronously waiting on a potential asynchronous connection");
                return manager.CreateAsync(context, metadata).GetAwaiter().GetResult();
            });
        }

        protected override ISessionState State => _state.Value;

        protected override void Dispose(bool disposing)
        {
            if (_state.IsValueCreated)
            {
                base.Dispose(disposing);
            }
        }
    }
}
