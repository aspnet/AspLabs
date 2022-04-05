// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.Adapters.SessionState;

public static class SystemWebAdapterExtensions
{
    private const string RemoteAppSessionOptions = "system-web-adapter-remote-app-session-options";

    internal static RemoteAppSessionStateOptions? GetRemoteSessionOptions(this HttpApplicationState state)
        => state.Get<RemoteAppSessionStateOptions>(RemoteAppSessionOptions);

    public static HttpApplicationState ConfigureRemoteSession(this HttpApplicationState state, Action<RemoteAppSessionStateOptions> configure)
        => state.ConfigureState(RemoteAppSessionOptions, configure);

    private static T? Get<T>(this HttpApplicationState state, string name) => state[name] is T result ? result : default;

    private static HttpApplicationState ConfigureState<T>(this HttpApplicationState state, string name, Action<T> configure)
        where T : new()
    {
        if (state[name] is not T existing)
        {
            existing = new();
            state[name] = existing;
        }

        configure(existing);

        return state;
    }
}
