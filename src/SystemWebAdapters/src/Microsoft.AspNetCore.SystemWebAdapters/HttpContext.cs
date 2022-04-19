// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Security.Principal;
using System.Web.Caching;
using System.Web.SessionState;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;

namespace System.Web;

public class HttpContext : IServiceProvider
{
    private static readonly HttpContextAccessor _accessor = new();

    private readonly HttpContextCore _context;

    private HttpRequest? _request;
    private HttpResponse? _response;
    private HttpServerUtility? _server;
    private IDictionary? _items;

    public static HttpContext? Current => _accessor.HttpContext;

    public HttpContext(HttpContextCore context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public HttpRequest Request => _request ??= new(_context.Request);

    public HttpResponse Response => _response ??= new(_context.Response);

    public IDictionary Items => _items ??= _context.Items.AsNonGeneric();

    public HttpServerUtility Server => _server ??= new(_context);

    public Cache Cache => _context.RequestServices.GetRequiredService<Cache>();

    public IPrincipal User
    {
        get => _context.User;
        set => _context.User = value is ClaimsPrincipal claims ? claims : new ClaimsPrincipal(value);
    }

    public HttpSessionState? Session => _context.Features.Get<HttpSessionState>();

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
