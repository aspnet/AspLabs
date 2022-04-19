// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Web;
using Microsoft.AspNetCore.SystemWebAdapters.Modules;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public static class SystemWebAdaptersExtensions
{
    private const string Key = "system-web-adapter-builder";

    public static ISystemWebAdapterBuilder AddSystemWebAdapters(this HttpApplicationState state)
    {
        if (state[Key] is not ISystemWebAdapterBuilder builder)
        {
            builder = new Builder();
            state[Key] = builder;
        }

        return builder;
    }

    public static ISystemWebAdapterBuilder AddProxySupport(this ISystemWebAdapterBuilder builder, Action<ProxyOptions> configure)
        => builder.AddModule(configure, static options => new ProxyHeaderModule(options));

    public static ISystemWebAdapterBuilder AddRemoteSession(this ISystemWebAdapterBuilder builder, Action<RemoteAppSessionStateOptions> configure)
        => builder.AddModule(configure, static options => new RemoteSessionModule(options));

    internal static ISystemWebAdapterBuilder? GetSystemWebBuilder(this HttpApplicationState state)
        => state[Key] as ISystemWebAdapterBuilder;

    private static ISystemWebAdapterBuilder AddModule<TOptions>(this ISystemWebAdapterBuilder builder, Action<TOptions> configure, Func<TOptions, IHttpModule> factory)
        where TOptions : class, new()
    {
        var options = new TOptions();
        configure(options);

        return builder.AddModule(factory(options));
    }

    private static ISystemWebAdapterBuilder AddModule(this ISystemWebAdapterBuilder builder, IHttpModule module)
    {
        builder.Modules.Add(module);
        return builder;
    }

    private class Builder : ISystemWebAdapterBuilder
    {
        public ICollection<IHttpModule> Modules { get; } = new List<IHttpModule>();
    }
}
