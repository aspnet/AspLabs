// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace System.Web.Adapters.SessionState;

internal class RemoteAppSessionStateManager : ISessionManager
{
    private readonly IOptionsMonitor<RemoteAppSessionStateOptions> _options;
    private readonly IHttpClientFactory _clientFactory;
    private readonly SessionSerializer _serializer;

    public RemoteAppSessionStateManager(
        IOptionsMonitor<RemoteAppSessionStateOptions> options,
        IHttpClientFactory clientFactory,
        SessionSerializer serializer)
    {
        _options = options;
        _clientFactory = clientFactory;
        _serializer = serializer;
    }

    public async Task<ISessionState> CreateAsync(HttpContextCore context, ISessionMetadata metadata)
    {
        var options = _options.CurrentValue;

        using var message = PrepareHttpRequestMessage(context, options, metadata);
        using var client = _clientFactory.CreateClient();

        using var response = await client.SendAsync(message, context.RequestAborted);

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();

        return await _serializer.DeserializeAsync(stream) ?? throw new InvalidOperationException("No session was found.");
    }

    private static HttpRequestMessage PrepareHttpRequestMessage(HttpContextCore context, RemoteAppSessionStateOptions options, ISessionMetadata metadata)
    {
        var message = new HttpRequestMessage(HttpMethod.Get, options.RemoteAppUrl);

        SetApiKey(message, options);
        AddAspNetCookie(message, context);
        SetReadOnly(message, metadata);

        return message;
    }

    private static void SetReadOnly(HttpRequestMessage message, ISessionMetadata metadata)
    {
        message.Headers.TryAddWithoutValidation(RemoteAppSessionStateOptions.ReadOnlyHeaderName, metadata.IsReadOnly.ToString());
    }

    private static void SetApiKey(HttpRequestMessage message, RemoteAppSessionStateOptions options)
    {
        if (options.ApiKey is not null)
        {
            message.Headers.TryAddWithoutValidation(options.ApiKeyHeader, options.ApiKey);
        }
    }

    private static void AddAspNetCookie(HttpRequestMessage message, HttpContextCore context)
    {
        if (context.Request.Headers.TryGetValue(HeaderNames.Cookie, out var cookies))
        {
            message.Headers.TryAddWithoutValidation(HeaderNames.Cookie, cookies.ToString());
        }
    }
}
