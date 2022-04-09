// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web.Adapters.SessionState;
using Microsoft.Extensions.DependencyInjection;

namespace System.Web.Adapters;

public static class RemoteAppSessionStateExtensions
{
    public static ISystemWebAdapterBuilder AddRemoteAppSession(this ISystemWebAdapterBuilder builder, Action<RemoteAppSessionStateOptions> configure)
    {
        builder.Services.AddSingleton<SessionSerializer>();
        builder.Services.AddHttpClient();
        builder.AddSessionManager<RemoteAppSessionStateManager>();
        builder.Services.AddOptions<RemoteAppSessionStateOptions>()
            .Configure(configure)
            .ValidateDataAnnotations();

        return builder;
    }
}
