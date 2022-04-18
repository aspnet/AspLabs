using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace System.Web.Adapters.SessionState;

internal class RemoteSessionService
{
    private readonly HttpClient _client;
    private readonly SessionSerializer _serializer;
    private readonly ILogger<RemoteSessionService> _logger;
    private readonly RemoteAppSessionStateOptions _options;

    public RemoteSessionService(HttpClient client, ILogger<RemoteSessionService> logger, SessionSerializer serializer, IOptions<RemoteAppSessionStateOptions> options)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _client.BaseAddress = new Uri(_options.RemoteApp, _options.SessionEndpointPath);
        _client.DefaultRequestHeaders.Add(_options.ApiKeyHeader, _options.ApiKey);
    }

    public async Task<RemoteSessionDataResponse> GetSessionDataAsync(string? sessionId, bool readOnly, HttpContextCore callingContext, CancellationToken cancellationToken = default)
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
        var json = await streamReader.ReadLineAsync();
        var remoteSessionState = _serializer.Deserialize(json);

        // Propagate headers back to the caller since a new session ID may have been set
        // by the remote app if there was no session active previously or if the previous
        // session expired.
        PropagateHeaders(response, callingContext, HeaderNames.SetCookie);

        if (remoteSessionState is null)
        {
            throw new InvalidOperationException("Failed to retrieve session state from remote session; confirm session is enabled in remote app");
        }

        return new RemoteSessionDataResponse(remoteSessionState, response);
    }

    public async Task SetOrReleaseSessionData(string sessionId, RemoteSessionData? remoteSessionData, CancellationToken cancellationToken = default)
    {
        var req = new HttpRequestMessage { Method = HttpMethod.Put };
        AddSessionCookieToHeader(req, sessionId);

        if (remoteSessionData is not null)
        {
            // Using JsonContent.Create would simplify this if the library drops
            // .NET Core 3.1 in the future.
            var memoryStream = new MemoryStream();
            await _serializer.SerializeAsync(remoteSessionData, memoryStream, cancellationToken);
            memoryStream.Position = 0;
            req.Content = new StreamContent(memoryStream);
            req.Content.Headers.ContentType = new("application/json") { CharSet = "utf-8" };
        }

        using var response = await _client.SendAsync(req, cancellationToken);
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
