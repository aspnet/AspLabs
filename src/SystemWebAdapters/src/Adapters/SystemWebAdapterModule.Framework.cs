// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web.Adapters.SessionState;
using System.Web.SessionState;

namespace System.Web.Adapters;

public sealed class SystemWebAdapterModule : IHttpModule, IReadOnlySessionState
{
    public void Dispose()
    {
    }

    public void Init(HttpApplication context)
    {
        RegisterRemoteSession(context);
    }

    private void RegisterRemoteSession(HttpApplication context)
    {
        if (context.Application.GetRemoteSessionOptions() is not { } options)
        {
            return;
        }

        var handler = new RemoteAppSessionStateHandler(options);

        context.PostMapRequestHandler += MapRemoteSessionHandler;

        void MapRemoteSessionHandler(object sender, EventArgs e)
        {
            var context = ((HttpApplication)sender).Context;

            if (string.Equals(context.Request.Path, options.SessionEndpointPath))
            {
                context.SetSessionStateBehavior(SessionStateBehavior.ReadOnly);
                context.Handler = handler;
            }
        }
    }
}
