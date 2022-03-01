// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Security.Principal;
using System.Web.Caching;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace System.Web
{
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
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            _context = context;
        }

        public HttpRequest Request
        {
            get
            {
                if (_request is null)
                {
                    _request = new(_context.Request);
                }

                return _request;
            }
        }

        public HttpResponse Response
        {
            get
            {
                if (_response is null)
                {
                    _response = new(_context.Response);
                }

                return _response;
            }
        }

        public IDictionary Items
        {
            get
            {
                if (_items is null)
                {
                    _items = _context.Items.AsNonGeneric();
                }

                return _items;
            }
        }

        public HttpServerUtility Server
        {
            get
            {
                if (_server is null)
                {
                    _server = new(_context);
                }

                return _server;
            }
        }

        public Cache Cache => _context.RequestServices.GetRequiredService<Cache>();

        public IPrincipal User
        {
            get => _context.User;
            set => _context.User = value is ClaimsPrincipal claims ? claims : new ClaimsPrincipal(value);
        }

        public object? GetService(Type service)
        {
            if (service == typeof(HttpRequest))
            {
                return Request;
            }
            else if (service == typeof(HttpResponse))
            {
                return Response;
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
}
