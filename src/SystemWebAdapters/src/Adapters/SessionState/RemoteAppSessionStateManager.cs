// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.SessionState;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace System.Web.Adapters.SessionState;

internal class RemoteAppSessionStateManager : ISessionManager, IDisposable
{
    private readonly IOptionsMonitor<RemoteAppSessionStateOptions> _options;
    private readonly HttpClient _httpClient;
    private readonly SessionSerializer _serializer;
    private readonly ILogger<RemoteAppSessionStateManager> _logger;
    private readonly SemaphoreSlim _remoteLoadSemaphore = new(1, 1);

    private HttpSessionState? _session;
    private HttpResponseMessage? _responseMessage;

    public RemoteAppSessionStateManager(
        IOptionsMonitor<RemoteAppSessionStateOptions> options,
        HttpClient httpClient,
        SessionSerializer serializer,
        ILogger<RemoteAppSessionStateManager> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<HttpSessionState> LoadAsync(HttpContextCore context, bool readOnly)
    {
        if (_session is null)
        {
            using var timeout = new CancellationTokenSource(_options.CurrentValue.NetworkTimeout);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, context.RequestAborted);

            // Ensure that session state is only loaded once, even if multiple threads attempt to access session state simultaneously.
            // Even though ASP.NET Core request handling is usually expected to be single-threaded, it's good to guarantee only one thread
            // loads session data since if two threads were to make the call, one would end up blocked waiting for the session state to
            // unlock.
            await _remoteLoadSemaphore.WaitAsync(cts.Token).ConfigureAwait(false);

            try
            {
                if (_session is null)
                {
                    // HttpCompletionOption.ResponseHeadersRead is important so that this call doesn't block
                    // waiting for the response to complete. It is expected that the response won't complete
                    // until the follow-up PUT call from CommitAsync.
                    _responseMessage = await _httpClient.SendAsync(PrepareReadRequest(context, readOnly), HttpCompletionOption.ResponseHeadersRead, cts.Token).ConfigureAwait(false);
                    _logger.LogTrace("Received {StatusCode} response loading remote session state", _responseMessage.StatusCode);
                    _responseMessage.EnsureSuccessStatusCode();

                    // Propagate headers back to the caller since a new session ID may have been set
                    // by the remote app if there was no session active previously or if the previous
                    // session expired.
                    PropagateHeaders(_responseMessage, context.Response, HeaderNames.SetCookie);

                    // Only read until the first new line since the response is expected to remain open until
                    // RemoteAppSessionStateManager.CommitAsync is called.
                    using var streamReader = new StreamReader(await _responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false));
                    var json = await streamReader.ReadLineAsync().ConfigureAwait(false);
                    var remoteSessionState = _serializer.DeserializeSessionState(json);

                    if (remoteSessionState is null)
                    {
                        throw new InvalidOperationException("Failed to retrieve session state from remote session; confirm session is enabled in remote app");
                    }
                    else
                    {
                        _logger.LogDebug("Loaded {SessionItemCount} items from remote session state for session {SessionId}", remoteSessionState.Count, remoteSessionState.SessionID);
                        _session = new HttpSessionState(remoteSessionState);
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unable to load remote session state");
                throw;
            }
            finally
            {
                _remoteLoadSemaphore.Release();
            }
        }

        return _session;
    }

    public async Task CommitAsync(HttpContextCore context)
    {
        if (_session is null)
        {
            return;
        }

        using var timeout = new CancellationTokenSource(_options.CurrentValue.NetworkTimeout);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, context.RequestAborted);

        try
        {
            // Mark the session complete so that no additional changes can be made
            _session.Complete();

            using var request = await PrepareWriteRequestAsync(_session, cts.Token).ConfigureAwait(false);
            using var response = await _httpClient.SendAsync(request, cts.Token).ConfigureAwait(false);
            _logger.LogTrace("Received {StatusCode} response committing remote session state", response.StatusCode);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception exc)
        {
            _logger.LogError(exc, "Unable to commit remote session state for session {SessionKey}", _session.SessionID);
            throw;
        }
        finally
        {
            _responseMessage?.Dispose();
            _responseMessage = null;
        }
    }

    public void Dispose()
    {
        _responseMessage?.Dispose();
    }

    private HttpRequestMessage PrepareReadRequest(HttpContextCore context, bool readOnly)
    {
        var options = _options.CurrentValue;
        var cookie = GetAspNetSessionCookie(context, options);
        var message = new HttpRequestMessage(HttpMethod.Get, new Uri(options.RemoteApp, options.SessionEndpointPath));

        if (cookie is not null)
        {
            SetAspNetSessionCookie(message, options, cookie);
        }

        SetReadOnly(message, readOnly);
        SetApiKey(message, options);

        return message;
    }

    private async Task<HttpRequestMessage> PrepareWriteRequestAsync(HttpSessionState session, CancellationToken token)
    {
        var options = _options.CurrentValue;
        var message = new HttpRequestMessage(HttpMethod.Put, new Uri(options.RemoteApp, options.SessionEndpointPath));

        // Don't get the cookie from the current context since the session ID may have been set or changed
        // by the initial call to retrieve session state; instead, construct the cookie based on current
        // session ID
        SetAspNetSessionCookie(message, options, session.SessionID);
        SetApiKey(message, options);

        if (session.HasUpdates)
        {
            var memoryStream = new MemoryStream();
            await _serializer.SerializeAsync(session.Updates, memoryStream, token).ConfigureAwait(false);
            memoryStream.Position = 0;
            message.Content = new StreamContent(memoryStream);
            message.Content.Headers.ContentType = new("application/json") { CharSet = "utf-8" };
        }

        return message;
    }

    private static void PropagateHeaders(HttpResponseMessage responseMessage, HttpResponseCore response, string cookieName)
    {
        if (responseMessage.Headers.TryGetValues(cookieName, out var cookieValues))
        {
            response.Headers.Add(cookieName, cookieValues.ToArray());
        }
    }

    private static void SetReadOnly(HttpRequestMessage message, bool readOnly)
        => message.Headers.TryAddWithoutValidation(RemoteAppSessionStateOptions.ReadOnlyHeaderName, readOnly.ToString());

    private static void SetApiKey(HttpRequestMessage message, RemoteAppSessionStateOptions options)
        => message.Headers.TryAddWithoutValidation(options.ApiKeyHeader, options.ApiKey);

    private static string? GetAspNetSessionCookie(HttpContextCore context, RemoteAppSessionStateOptions options)
        => context.Request.Cookies.TryGetValue(options.CookieName, out var cookie) ? cookie : null;

    private static void SetAspNetSessionCookie(HttpRequestMessage message, RemoteAppSessionStateOptions options, string cookie)
        => message.Headers.TryAddWithoutValidation(HeaderNames.Cookie, $"{options.CookieName}={cookie}");
}
