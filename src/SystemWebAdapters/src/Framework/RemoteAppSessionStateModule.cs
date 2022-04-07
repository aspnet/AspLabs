// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web.SessionState;

namespace System.Web.Adapters.SessionState;

/// <summary>
/// Custom module for enabling session and enabling a custom handler
/// for requests sent to remote session store/commit endpoints.
/// </summary>
public sealed class RemoteAppSessionStateModule : IHttpModule
{
    public void Dispose() { }

    public void Init(HttpApplication application)
    {
        application.PostMapRequestHandler += Application_PostMapRequestHandler;
    }

    private void Application_PostMapRequestHandler(object sender, EventArgs e)
    {
        var context = ((HttpApplication)sender).Context;

        // Check whether the request is for the session state path
        if (context.Request.AppRelativeCurrentExecutionFilePath.Equals("~/session-state", StringComparison.OrdinalIgnoreCase))
        {
            // Set the handler before setting session state behavior since
            // setting the handler can override session state behavior
            context.Handler = new RemoteAppSessionStateHandler();

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

    private bool GetExclusiveParameter(HttpRequest request)
        => !string.Equals(request.Headers.Get(RemoteAppSessionStateOptions.ReadOnlyHeaderName), true.ToString(), StringComparison.OrdinalIgnoreCase);
}
