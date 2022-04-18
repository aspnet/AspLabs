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
                // Set the handler before setting session state behavior since
                // setting the handler can override session state behavior
                context.Handler = handler;

                var exclusive = GetExclusiveParameter(context.Request);
                var httpMethod = context.Request.HttpMethod;

                // Getting the session state with exclusive access requires session
                if (exclusive && httpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
                {
                    context.SetSessionStateBehavior(SessionStateBehavior.Required);
                }

                // Getting the session with non-exclusive access requires read-only session
                else if (!exclusive && httpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
                {
                    context.SetSessionStateBehavior(SessionStateBehavior.ReadOnly);
                }

                // Setting session with exclusive access requires NO session because the session
                // should have already been previously locked by an exclusive get call.
                // PUT calls should typically be exclusive, but callers might pass !exclusive
                // if there are no updates being made and the call is only made to release a lock
                // on read-only session state.
                else if (httpMethod.Equals("PUT", StringComparison.OrdinalIgnoreCase))
                {
                    context.SetSessionStateBehavior(SessionStateBehavior.Disabled);
                }
            }
        }
    }

    public void Dispose()
    {
    }

    private bool GetExclusiveParameter(HttpRequest request)
    => !(bool.TryParse(request.Headers.Get(RemoteAppSessionStateOptions.ReadOnlyHeaderName), out var result) && result);
}
