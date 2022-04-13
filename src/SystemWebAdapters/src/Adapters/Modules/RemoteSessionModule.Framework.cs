// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web.Adapters.SessionState;
using System.Web.SessionState;

namespace System.Web.Adapters;

internal sealed class RemoteSessionModule : IHttpModule
{
    private readonly RemoteAppSessionStateOptions _options;

    public RemoteSessionModule(RemoteAppSessionStateOptions options)
    {
        _options = options;
    }

    public void Init(HttpApplication context)
    {
        var handler = new RemoteAppSessionStateHandler(_options);

        context.PostMapRequestHandler += MapRemoteSessionHandler;

        void MapRemoteSessionHandler(object sender, EventArgs e)
        {
            var context = ((HttpApplication)sender).Context;

            if (string.Equals(context.Request.Path, _options.SessionEndpointPath))
            {
                context.SetSessionStateBehavior(SessionStateBehavior.ReadOnly);
                context.Handler = handler;
            }
        }
    }

    public void Dispose()
    {
    }
}
