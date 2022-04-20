// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Caching;
using System.Web.SessionState;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters;

namespace System.Web;

public class HttpContext : IServiceProvider
{
    private static readonly HttpContextAccessor _accessor = new();

    private readonly HttpContextCore _context;

#if NET6_0_OR_GREATER
    internal static ActivitySource Source { get; } = new("Microsoft.AspNetCore.SystemWebAdapters");

    private readonly Activity? _activity;
#endif

    private HttpRequest? _request;
    private HttpResponse? _response;
    private HttpServerUtility? _server;
    private IDictionary? _items;

    public static HttpContext? Current => _accessor.HttpContext;

    public HttpContext(HttpContextCore context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));

#if NET6_0_OR_GREATER
        _activity = Source.StartActivity("HttpContext");

        if (_activity is not null)
        {
            if (_context.GetEndpoint() is { } endpoint)
            {
                _activity.AddTag("Endpoint", endpoint);
            }

            context.Response.OnCompleted(static state =>
            {
                ((Activity)state).Stop();
                return Task.CompletedTask;
            }, _activity);
        }
#endif
    }

    public HttpRequest Request
    {
        get
        {
#if NET6_0_OR_GREATER
            _activity?.AddEvent(new ActivityEvent(nameof(Request)));
#endif
            return _request ??= new(_context.Request);
        }
    }

    public HttpResponse Response
    {
        get
        {
#if NET6_0_OR_GREATER
            _activity?.AddEvent(new ActivityEvent(nameof(Response)));
#endif
            return _response ??= new(_context.Response);
        }
    }

    public IDictionary Items
    {
        get
        {
#if NET6_0_OR_GREATER
            _activity?.AddEvent(new ActivityEvent(nameof(Items)));
#endif
            return _items ??= _context.Items.AsNonGeneric();
        }
    }

    public HttpServerUtility Server
    {
        get
        {
#if NET6_0_OR_GREATER
            _activity?.AddEvent(new ActivityEvent(nameof(Server)));
#endif
            return _server ??= new(_context);
        }
    }

    public Cache Cache => throw new NotImplementedException();

    public IPrincipal User
    {
        get => _context.User;
        set => _context.User = value is ClaimsPrincipal claims ? claims : new ClaimsPrincipal(value);
    }

    public HttpSessionState? Session
    {
        get
        {
#if NET6_0_OR_GREATER
            _activity?.AddEvent(new ActivityEvent(nameof(Session)));
#endif
            return _context.Features.Get<HttpSessionState>();
        }
    }

    object? IServiceProvider.GetService(Type service)
    {
        if (service == typeof(HttpRequest))
        {
            return Request;
        }
        else if (service == typeof(HttpResponse))
        {
            return Response;
        }
        else if (service == typeof(HttpSessionState))
        {
            return Session;
        }
        else if (service == typeof(HttpServerUtility))
        {
            return Server;
        }

        return null;
    }

    [return: NotNullIfNotNull("context")]
    public static implicit operator HttpContext?(HttpContextCore? context) => context?.GetAdapter();

    [return: NotNullIfNotNull("context")]
    public static implicit operator HttpContextCore?(HttpContext? context) => context?._context;
}
