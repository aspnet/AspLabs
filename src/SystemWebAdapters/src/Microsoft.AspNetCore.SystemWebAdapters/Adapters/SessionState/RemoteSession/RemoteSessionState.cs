// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

internal sealed class RemoteSessionState : ISessionState
{
    private readonly RemoteSessionService _remoteSessionService;
    private readonly RemoteAppSessionStateOptions _options;
    private readonly ILogger<RemoteSessionState> _logger;
    private readonly RemoteSessionDataResponse _remoteDataResponse;

    private int _committed;

    private RemoteSessionData RemoteData => _remoteDataResponse.RemoteSessionData;

    private RemoteSessionState(RemoteSessionDataResponse remoteDataResponse, RemoteSessionService remoteSessionService, RemoteAppSessionStateOptions options, ILogger<RemoteSessionState> logger)
    {
        _remoteDataResponse = remoteDataResponse ?? throw new ArgumentNullException(nameof(remoteDataResponse));
        _remoteSessionService = remoteSessionService ?? throw new ArgumentNullException(nameof(remoteSessionService));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public static async Task<RemoteSessionState> CreateAsync(HttpContextCore context,
                                                      bool readOnly,
                                                      RemoteSessionService remoteSessionService,
                                                      RemoteAppSessionStateOptions options,
                                                      ILogger<RemoteSessionState> logger,
                                                      CancellationToken cancellationToken = default)
    {
        using var timeout = new CancellationTokenSource(options.NetworkTimeout);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, context.RequestAborted, cancellationToken);

        // If an existing remote session ID is present in the request, use its session ID.
        // Otherwise, leave session ID null for now; it will be provided by the remote service
        // when session data is loaded.
        var sessionId = context.Request.Cookies[options.CookieName];

        try
        {
            // Get or create session data
            var response = await remoteSessionService.GetSessionDataAsync(sessionId, readOnly, context, cts.Token);
            logger.LogDebug("Loaded {SessionItemCount} items from remote session state for session {SessionId}", response.RemoteSessionData.Values.Count, sessionId);

            return new RemoteSessionState(response, remoteSessionService, options, logger);
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Unable to load remote session state for session {SessionId}", sessionId);
            throw;
        }
    }

    // Commit changes to session state and release the session lock
    public ValueTask CommitAsync(CancellationToken cancellationToken = default)
        => CommitAsync(RemoteData, cancellationToken);

    // Commits changes to the server. Passing null RemoteSessionData will release the session lock
    // but not update session data.
    private async ValueTask CommitAsync(RemoteSessionData? remoteData, CancellationToken cancellationToken = default)
    {
        if (Interlocked.Exchange(ref _committed, 1) == 1)
        {
            // Already committed
            return;
        }

        if (RemoteData.IsReadOnly)
        {
            _logger.LogDebug("Skipping commit for read-only session");
            return;
        }

        using var timeout = new CancellationTokenSource(_options.NetworkTimeout);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);

        var sessionId = RemoteData.SessionID;

        try
        {
            await _remoteSessionService.SetOrReleaseSessionData(sessionId, remoteData, cts.Token);
            _logger.LogDebug("Set items and released lock for session {SessionId}", sessionId);
        }
        catch (Exception exc)
        {
            _logger.LogError(exc, "Unable to commit remote session state for session {SessionKey}", sessionId);
            throw;
        }
        finally
        {
            _remoteDataResponse.Dispose();
        }
    }

    public string SessionID => RemoteData.SessionID;

    public object? this[string key]
    {
        get => RemoteData.Values[key];
        set => RemoteData.Values[key] = value;
    }

    public int Timeout
    {
        get => RemoteData.Timeout;
        set => RemoteData.Timeout = value;
    }

    public bool IsNewSession => RemoteData.IsNewSession;

    public IEnumerable<string> Keys => RemoteData.Values.Keys;

    public int Count => RemoteData.Values.Count;

    public bool IsReadOnly => RemoteData.IsReadOnly;

    public bool IsSynchronized => false;

    public object SyncRoot => this;

    public void Abandon() => RemoteData.IsAbandoned = true;

    public void Add(string key, object? value) => RemoteData.Values[key] = value;

    public void Clear() => RemoteData.Values.Clear();

    public void Remove(string key) => RemoteData.Values.Remove(key);

    public void CopyTo(Array array, int index) => ((ICollection)RemoteData.Values).CopyTo(array, index);

    // Release the session lock (if necessary) without submitting any session updates
    public ValueTask DisposeAsync() =>
        CommitAsync(null, default);
}
