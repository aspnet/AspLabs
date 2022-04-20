// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

internal class RemoteAppSessionStateManager : ISessionManager
{
    private readonly HttpClient _client;
    private readonly ISessionSerializer _serializer;
    private readonly ILogger<RemoteAppSessionStateManager> _logger;
    private readonly RemoteAppSessionStateOptions _options;

    public RemoteAppSessionStateManager(
        HttpClient client,
        ISessionSerializer serializer,
        IOptions<RemoteAppSessionStateOptions> options,
        ILogger<RemoteAppSessionStateManager> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _client.BaseAddress = new Uri(_options.RemoteApp, _options.SessionEndpointPath);
        _client.DefaultRequestHeaders.Add(_options.ApiKeyHeader, _options.ApiKey);
    }

    public async Task<ISessionState> CreateAsync(HttpContextCore context, ISessionMetadata metadata)
    {
        using var timeout = new CancellationTokenSource(_options.NetworkTimeout);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, context.RequestAborted, context.RequestAborted);

        // If an existing remote session ID is present in the request, use its session ID.
        // Otherwise, leave session ID null for now; it will be provided by the remote service
        // when session data is loaded.
        var sessionId = context.Request.Cookies[_options.CookieName];

        try
        {
            // Get or create session data
            var response = await GetSessionDataAsync(sessionId, metadata.IsReadOnly, context, cts.Token);

            _logger.LogDebug("Loaded {SessionItemCount} items from remote session state for session {SessionId}", response.Count, response.SessionID);

            return response;
        }
        catch (Exception exc)
        {
            _logger.LogError(exc, "Unable to load remote session state for session {SessionId}", sessionId);
            throw;
        }
    }

    private async Task<ISessionState> GetSessionDataAsync(string? sessionId, bool readOnly, HttpContextCore callingContext, CancellationToken cancellationToken = default)
    {
        var req = new HttpRequestMessage { Method = HttpMethod.Get };
        AddSessionCookieToHeader(req, sessionId);
        AddReadOnlyHeader(req, readOnly);

        var response = await _client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        _logger.LogTrace("Received {StatusCode} response getting remote session state for request {RequestUrl}", response.StatusCode, callingContext.Request.GetDisplayUrl());
        response.EnsureSuccessStatusCode();

        // Only read until the first new line since the response is expected to remain open until
        // RemoteAppSessionStateManager.CommitAsync is called.
        using var streamReader = new StreamReader(await response.Content.ReadAsStreamAsync());
        var remoteSessionState = await ReadSessionState(streamReader);

        // Propagate headers back to the caller since a new session ID may have been set
        // by the remote app if there was no session active previously or if the previous
        // session expired.
        PropagateHeaders(response, callingContext, HeaderNames.SetCookie);

        if (remoteSessionState is null)
        {
            throw new InvalidOperationException("Failed to retrieve session state from remote session; confirm session is enabled in remote app");
        }

        return new RemoteSessionState(remoteSessionState, response, SetOrReleaseSessionData);
    }

    private async Task<ISessionState> ReadSessionState(StreamReader streamReader)
    {
        var json = await streamReader.ReadLineAsync();

        return _serializer.Deserialize(json) ?? throw new InvalidOperationException("Could not retrieve session state");
    }

    /// <summary>
    /// Commits changes to the server. Passing null <paramref name="state"/> will release the session lock but not update session data.
    /// </summary>
    private async Task SetOrReleaseSessionData(ISessionState? state, CancellationToken cancellationToken)
    {
        using var timeout = new CancellationTokenSource(_options.NetworkTimeout);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);

        using var req = new HttpRequestMessage { Method = HttpMethod.Put };

        if (state is not null)
        {
            AddSessionCookieToHeader(req, state.SessionID);

            var bytes = _serializer.Serialize(state);
            req.Content = new ByteArrayContent(bytes);
            req.Content.Headers.ContentType = new("application/json") { CharSet = "utf-8" };
        }

        using var response = await _client.SendAsync(req, cts.Token);
        _logger.LogTrace("Received {StatusCode} response committing remote session state", response.StatusCode);
        response.EnsureSuccessStatusCode();
    }

    private static void PropagateHeaders(HttpResponseMessage responseMessage, HttpContextCore context, string cookieName)
    {
        if (context?.Response is not null && responseMessage.Headers.TryGetValues(cookieName, out var cookieValues))
        {
            context.Response.Headers.Add(cookieName, cookieValues.ToArray());
        }
    }

    private void AddSessionCookieToHeader(HttpRequestMessage req, string? sessionId)
    {
        if (!string.IsNullOrEmpty(sessionId))
        {
            req.Headers.Add(HeaderNames.Cookie, $"{_options.CookieName}={sessionId}");
        }
    }

    private static void AddReadOnlyHeader(HttpRequestMessage req, bool readOnly)
        => req.Headers.Add(RemoteAppSessionStateOptions.ReadOnlyHeaderName, readOnly.ToString());
}
