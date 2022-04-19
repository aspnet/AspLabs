// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters.Modules;

internal class ProxyHeaderModule : IHttpModule
{
    private readonly ProxyOptions _options;

    public ProxyHeaderModule(ProxyOptions options)
    {
        _options = options;
    }

    public void Dispose()
    {
    }

    public void Init(HttpApplication context)
    {
        if (_options.UseForwardedHeaders)
        {
            context.BeginRequest += static (s, e) => UseHeaders(((HttpApplication)s).Context.Request);
        }
        else
        {
            if (_options.ServerName is null)
            {
                throw new InvalidOperationException("Server name must be set for proxy options.");
            }

            context.BeginRequest += (s, e) => UseOptions(((HttpApplication)s).Context.Request);
        }
    }

    private void UseOptions(HttpRequest request)
    {
        UseForwardedFor(request);

        request.ServerVariables.Set("SERVER_NAME", _options.ServerName);
        request.ServerVariables.Set("SERVER_PORT", _options.ServerPortString);
        request.ServerVariables.Set("SERVER_PROTOCOL", _options.Scheme);
    }

    private static void UseHeaders(HttpRequest request)
    {
        UseForwardedFor(request);

        if (request.Headers["x-forwarded-host"] is { } host)
        {
            var value = new ForwardedHost(host);

            request.ServerVariables.Set("SERVER_NAME", value.ServerName);

            if (value.Port is { })
            {
                request.ServerVariables.Set("SERVER_PORT", value.Port);
            }
        }

        if (request.Headers["x-forwarded-proto"] is { } proto)
        {
            request.ServerVariables.Set("SERVER_PROTOCOL", proto);
        }
    }

    private static void UseForwardedFor(HttpRequest request)
    {
        if (request.Headers["x-forwarded-for"] is { } remote)
        {
            request.ServerVariables.Set("REMOTE_ADDR", remote);
            request.ServerVariables.Set("REMOTE_HOST", remote);
        }
    }

    private struct ForwardedHost
    {
        public ForwardedHost(string host)
        {
            var idx = host.IndexOf(":");

            if (idx < 0)
            {
                ServerName = host;
                Port = null;
            }
            else
            {
                ServerName = host.Substring(0, idx);
                Port = host.Substring(idx + 1);
            }
        }

        public string ServerName { get; }

        public string? Port { get; }
    }
}
