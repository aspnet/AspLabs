// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

internal sealed class RemoteAppSessionStateHandler : HttpTaskAsyncHandler
{
    private readonly RemoteAppSessionStateOptions _options;
    private readonly SessionSerializer _serializer;

    // Track locked sessions awaiting updates or release
    private static readonly ConcurrentDictionary<string, StateContainer> SessionResponseTasks = new();

    public override bool IsReusable => true;

    public RemoteAppSessionStateHandler(RemoteAppSessionStateOptions options)
    {
        _options = options;
        _serializer = new SessionSerializer(options.KnownKeys);
    }

    public override async Task ProcessRequestAsync(HttpContext context)
    {
        if (_options.ApiKey is null || !string.Equals(_options.ApiKey, context.Request.Headers.Get(_options.ApiKeyHeader), StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = 401;
        }
        else
        {
            var readOnly = bool.TryParse(context.Request.Headers.Get(RemoteAppSessionStateOptions.ReadOnlyHeaderName), out var result) && result;

            // Dispatch the work depending on the HTTP method used
            var method = context.Request.HttpMethod;
            if (method.Equals("PUT", StringComparison.OrdinalIgnoreCase))
            {
                await StoreSessionStateAsync(context).ConfigureAwait(false);
            }
            else if (method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                await GetSessionStateAsync(context, !readOnly).ConfigureAwait(false);
            }
            else
            {
                // HTTP methods other than GET (read) or PUT (write) are not accepted
                context.Response.StatusCode = 405; // Method not allowed
            }
        }

        context.ApplicationInstance.CompleteRequest();
    }

    private async Task GetSessionStateAsync(HttpContext context, bool exclusive)
    {
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(context.Session.Timeout));
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, context.Response.ClientDisconnectedToken);

        if (exclusive)
        {
            await GetExclusiveSessionStateAsync(context, cts.Token);
        }
        else
        {
            await GetNonExclusiveSessionStateAsync(context, cts.Token);
        }
    }

    private async Task GetExclusiveSessionStateAsync(HttpContext context, CancellationToken token)
    {
        // If session data is retrieved exclusively, then it needs sent to the client and
        // this request needs to remain open while waiting for the client to either send updates
        // or release the session without updates.
        try
        {
            // Generate a channel to wait for session data updates
            using var sessionContainer = new StateContainer(context.Session);

            // Cancel the task if this request is cancelled or timed out
            using var cancellationRegistration = token.Register(() => sessionContainer.Complete());

            // Update the channels dictionary with the new channel
            SessionResponseTasks[context.Session.SessionID] = sessionContainer;

            // Send the initial snapshot of session data
            context.Response.ContentType = "text/event-stream";
            context.Response.StatusCode = 200;

            await _serializer.SerializeAsync(context.Session, context.Response.OutputStream, token);

            // Delimit the json body with a new line to mark the end of content
            context.Response.Write('\n');
            await context.Response.FlushAsync();

            // Wait for up to request timeout for updated session state to be written.
            // We send down heartbeats to ensure the request disconnected token fires correctly
            using var waitToken = CancellationTokenSource.CreateLinkedTokenSource(sessionContainer.Token, token);
            var heartbeatDelay = TimeSpan.FromMilliseconds(20);

            while (!waitToken.IsCancellationRequested)
            {
                await Task.Delay(heartbeatDelay, waitToken.Token);
                context.Response.Write(' ');
                await context.Response.FlushAsync();
            }
        }
        finally
        {
            SessionResponseTasks.TryRemove(context.Session.SessionID, out _);
        }
    }

    private async Task GetNonExclusiveSessionStateAsync(HttpContext context, CancellationToken token)
    {
        // If the session is retrieved non-exclusively, then no updates will be made and the
        // session state can be returned directly, completing this request.
        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.StatusCode = 200;

        await _serializer.SerializeAsync(context.Session, context.Response.OutputStream, token);
    }

    private async Task StoreSessionStateAsync(HttpContext context)
    {
        var sessionId = GetSessionId(context.Request);

        // Check that the request has a session ID
        if (sessionId is null)
        {
            context.Response.StatusCode = 400;
            context.Response.StatusDescription = "No session ID found";
            context.Response.End();
            return;
        }

        // Get the channel that will be used to write the updated session data
        // to the in-progress request that will update session data
        if (SessionResponseTasks.TryGetValue(sessionId, out var responseTask))
        {
            if (responseTask.State is { } state)
            {
                using var requestContent = context.Request.GetBufferlessInputStream();

                await _serializer.DeserializeToAsync(requestContent, state, context.Response.ClientDisconnectedToken);

                responseTask.Complete();
                context.Response.StatusCode = 200;
            }
            else
            {
                context.Response.StatusDescription = "Unable to update session state; state may have already been updated";
                context.Response.StatusCode = 400;
            }
        }
        else
        {
            context.Response.StatusDescription = "Specified session ID is not locked for writing";
            context.Response.StatusCode = 400;
        }
    }

    // context.Session will intentionally not be populated in PUT scenarios (to avoid needing a lock),
    // so read the session ID directly from the cookie instead
    private string? GetSessionId(HttpRequest request)
        => request.Cookies[_options.CookieName]?.Value;

    private sealed class StateContainer : IDisposable
    {
        private readonly CancellationTokenSource _done = new();

        public StateContainer(HttpSessionState state)
        {
            State = state;
        }

        public HttpSessionState? State { get; private set; }

        public void Complete()
        {
            _done.Cancel();
            State = null;
        }

        public void Dispose() => _done.Dispose();

        public CancellationToken Token => _done.Token;
    }
}
