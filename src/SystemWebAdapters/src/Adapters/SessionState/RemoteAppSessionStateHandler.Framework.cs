// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Web.SessionState;

namespace System.Web.Adapters.SessionState;

internal sealed class RemoteAppSessionStateHandler : HttpTaskAsyncHandler
{
    private readonly RemoteAppSessionStateOptions _options;
    private readonly SessionSerializer _serializer;

    // Track locked sessions awaiting updates or release
    private static readonly ConcurrentDictionary<string, Channel<ISessionUpdate?>> SessionDataChannels = new();

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
            var readOnly = string.Equals(context.Request.Headers.Get(RemoteAppSessionStateOptions.ReadOnlyHeaderName), true.ToString(), StringComparison.OrdinalIgnoreCase);

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
        var timeoutCts = new CancellationTokenSource(context.Server.ScriptTimeout * 1000);
        var cts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, context.Response.ClientDisconnectedToken);

        if (exclusive)
        {
            // If session data is retrieved exclusively, then it needs sent to the client and
            // this request needs to remain open while waiting for the client to either send updates
            // or release the session without updates.
            try
            {
                // Generate a channel to wait for session data updates
                var responseChannel = Channel.CreateBounded<ISessionUpdate?>(1);

                // Update the channels dictionary with the new channel
                SessionDataChannels[context.Session.SessionID] = responseChannel;

                // Send the initial snapshot of session data
                context.Response.ContentType = "text/event-stream";
                context.Response.StatusCode = 200;
                await _serializer.SerializeAsync(context.Session, context.Response.OutputStream, cts.Token).ConfigureAwait(false);

                // Delimit the json body with a new line to mark the end of content
                context.Response.Write('\n');
                await context.Response.FlushAsync().ConfigureAwait(false);

                // Wait for up to request timeout for updated session state to be provided
                // (or for null to be passed as updated session state to indicate no updates).
                var updatedSessionState = await responseChannel.Reader.ReadAsync(cts.Token).ConfigureAwait(false);
                if (updatedSessionState is not null)
                {
                    UpdateSessionState(context.Session, updatedSessionState);
                }
            }
            finally
            {
                SessionDataChannels.TryRemove(context.Session.SessionID, out _);
            }
        }
        else
        {
            // If the session is retrieved non-exclusively, then no updates will be made and the
            // session state can be returned directly, completing this request.
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.StatusCode = 200;
            await _serializer.SerializeAsync(context.Session, context.Response.OutputStream, cts.Token).ConfigureAwait(false);
        }
    }

    private async Task StoreSessionStateAsync(HttpContext context)
    {
        using var requestContent = context.Request.GetBufferlessInputStream();
        var sessionData = await _serializer.DeserializeSessionUpdateAsync(requestContent).ConfigureAwait(false);

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
        if (SessionDataChannels.TryGetValue(sessionId, out var channel))
        {
            if (channel.Writer.TryWrite(sessionData))
            {
                // Mark the WriteChannel as complete since only one session update is expected
                channel.Writer.TryComplete();
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

    private void UpdateSessionState(HttpSessionState session, ISessionUpdate updatedSessionState)
    {
        session.Timeout = updatedSessionState.Timeout ?? session.Timeout;

        foreach (var key in updatedSessionState.UpdatedKeys)
        {
            session[key] = updatedSessionState[key];
        }

        foreach (var removedItem in updatedSessionState.RemovedKeys)
        {
            session.Remove(removedItem);
        }

        if (updatedSessionState.Abandon)
        {
            session.Abandon();
        }
    }

    // context.Session will intentionally not be populated in PUT scenarios (to avoid needing a lock),
    // so read the session ID directly from the cookie instead
    private string? GetSessionId(HttpRequest request)
        => request.Cookies[_options.CookieName]?.Value;
}
