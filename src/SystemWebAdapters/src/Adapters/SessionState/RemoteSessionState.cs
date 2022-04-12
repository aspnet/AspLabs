using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace System.Web.Adapters.SessionState;

internal class RemoteSessionState : ISessionState
{
    private readonly RemoteSessionService _remoteSessionService;
    private readonly RemoteAppSessionStateOptions _options;
    private readonly ILogger<RemoteSessionState> _logger;
    private readonly SemaphoreSlim _remoteLoadSemaphore = new(1, 1);

    private bool _loaded;
    private HttpResponseMessage? _response;

    private RemoteSessionData? _remoteData;

    public RemoteSessionState(RemoteSessionService remoteSessionService, RemoteAppSessionStateOptions options, ILogger<RemoteSessionState> logger)
    {
        _remoteSessionService = remoteSessionService ?? throw new ArgumentNullException(nameof(remoteSessionService));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task LoadAsync(HttpContextCore context, bool readOnly, CancellationToken cancellationToken = default)
    {
        if (!_loaded)
        {
            using var timeout = new CancellationTokenSource(_options.NetworkTimeout);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, context.RequestAborted, cancellationToken);

            // Ensure that session state is only loaded once, even if multiple threads attempt to access session state simultaneously.
            // Even though ASP.NET Core request handling is usually expected to be single-threaded, it's good to guarantee only one thread
            // loads session data since if two threads were to make the call one would end up blocked waiting for the session state to
            // unlock.
            await _remoteLoadSemaphore.WaitAsync(cts.Token);

            // If an existing remote session ID is present in the request, use its session ID.
            // Otherwise, leave session ID null for now; it will be provided by the remote service
            // when session data is loaded.
            var sessionId = context.Request.Cookies[_options.CookieName];

            try
            {
                if (!_loaded)
                {
                    // Get or create session data
                    var response = await _remoteSessionService.GetSessionDataAsync(sessionId, readOnly, context, cts.Token);
                    _remoteData = response.RemoteSessionData;
                    _response = response.HttpRespone;
                    _logger.LogDebug("Loaded {SessionItemCount} items from remote session state for session {SessionId}", _remoteData.Values.Count, sessionId);
                }
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Unable to load remote session state for session {SessionId}", sessionId);
                throw;
            }
            finally
            {
                _loaded = true;
                _remoteLoadSemaphore.Release();
            }
        }
    }

    public async Task CommitAsync(HttpContextCore context, CancellationToken cancellationToken = default)
    {
        if (!_loaded || _remoteData is null)
        {
            _logger.LogInformation("No session available to commit");
            return;
        }

        if (_remoteData.IsReadOnly)
        {
            _logger.LogDebug("Skipping commit for read-only session");
            return;
        }

        using var timeout = new CancellationTokenSource(_options.NetworkTimeout);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, context.RequestAborted, cancellationToken);

        var sessionId = _remoteData.SessionID;

        try
        {
            // Note that this is not thread safe and callers should not be writing session state while committing
            await _remoteSessionService.SetOrReleaseSessionData(sessionId, _remoteData, cts.Token);
            _remoteData = null;
            _logger.LogDebug("Set items and released lock for session {SessionId}", sessionId);
        }
        catch (Exception exc)
        {
            _logger.LogError(exc, "Unable to commit remote session state for session {SessionKey}", sessionId);
            throw;
        }
        finally
        {
            _response?.Dispose();
            _response = null;
        }
    }

#if NET6_0_OR_GREATER
    [MemberNotNullWhen(true, nameof(_remoteData))]
#endif
    public bool IsAvailable => _remoteData is not null;

    public string SessionID
    {
        get
        {
            ThrowIfNotAvailable();
            return _remoteData.SessionID;
        }
    }

    public object? this[string key]
    {
        get
        {
            ThrowIfNotAvailable();
            return _remoteData.Values[key];
        }
        set
        {
            ThrowIfNotAvailable();
            _remoteData.Values[key] = value;
        }
    }

    public int Timeout
    {
        get
        {
            ThrowIfNotAvailable();
            return _remoteData.Timeout;
        }
        set
        {
            ThrowIfNotAvailable();
            _remoteData.Timeout = value;
        }
    }

    public bool IsNewSession
    {
        get
        {
            ThrowIfNotAvailable();
            return _remoteData.IsNewSession;
        }
    }

    public ICollection<string> Keys
    {
        get
        {
            ThrowIfNotAvailable();
            return _remoteData.Values.Keys.Cast<string>().ToList();
        }
    }

    public ICollection<object?> Values
    {
        get
        {
            ThrowIfNotAvailable();
            return _remoteData.Values.KeyValues.Select(kvp => kvp.Value).ToList();
        }
    }

    public int Count
    {
        get
        {
            ThrowIfNotAvailable();
            return _remoteData.Values.Count;
        }
    }

    public bool IsReadOnly
    {
        get
        {
            ThrowIfNotAvailable();
            return _remoteData.IsReadOnly;
        }
    }

    public void Abandon()
    {
        ThrowIfNotAvailable();
        _remoteData.Abandon = true;
    }

    public void Add(string key, object? value)
    {
        ThrowIfNotAvailable();
        _remoteData.Values[key] = value;
    }

    public void Add(KeyValuePair<string, object?> item) => Add(item.Key, item.Value);

    public void Clear()
    {
        ThrowIfNotAvailable();
        _remoteData.Values.Clear();
    }
    public bool Contains(KeyValuePair<string, object?> item)
    {
        ThrowIfNotAvailable();
        return _remoteData.Values.KeyValues.Select(kvp => KeyValuePair.Create(item.Key, item.Value)).Contains(item);
    }

    public bool ContainsKey(string key)
    {
        ThrowIfNotAvailable();
        return _remoteData.Values.Keys.Cast<string>().Contains(key);
    }

    public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex)
    {
        ThrowIfNotAvailable();
        foreach (var keyName in this)
        {
            array.SetValue(keyName, arrayIndex++);
        }
    }

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
    {
        ThrowIfNotAvailable();
        return _remoteData.Values.KeyValues.Select(kvp => KeyValuePair.Create(kvp.Key, kvp.Value)).GetEnumerator();
    }

    public bool Remove(string key)
    {
        ThrowIfNotAvailable();
        if (ContainsKey(key))
        {
            _remoteData.Values.Remove(key);
            return true;
        }

        return false;
    }

    public bool Remove(KeyValuePair<string, object?> item)
    {
        ThrowIfNotAvailable();
        if (Contains(item))
        {
            Remove(item.Key);
            return true;
        }

        return false;
    }

    public bool TryGetValue(string key, out object? value)
    {
        if (ContainsKey(key))
        {
            value = this[key];
            return true;
        }
        else
        {
            value = null;
            return false;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Dispose()
    {
        _response?.Dispose();
    }

#if NET6_0_OR_GREATER
    [MemberNotNull(nameof(_remoteData))]
#endif
    private void ThrowIfNotAvailable()
    {
        if (!IsAvailable)
        {
            throw new InvalidOperationException("No session state is currently loaded and uncommitted");
        }
    }
}
