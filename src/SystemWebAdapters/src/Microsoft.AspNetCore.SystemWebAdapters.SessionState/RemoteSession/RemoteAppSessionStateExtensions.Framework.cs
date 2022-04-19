// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public static class RemoteAppSessionStateExtensions
{
    public static ISystemWebAdapterBuilder AddRemoteAppSession(this ISystemWebAdapterBuilder builder, Action<RemoteAppSessionStateOptions> configure)
    {
        var options = new RemoteAppSessionStateOptions();
        configure(options);
        builder.Modules.Add(new RemoteSessionModule(options));
        return builder;
    }
}
