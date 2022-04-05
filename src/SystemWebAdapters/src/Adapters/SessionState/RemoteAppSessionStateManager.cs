// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
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
        if (!metadata.IsReadOnly)
        {
            throw new NotSupportedException("Only readonly session is currently supported");
        }

        var options = _options.CurrentValue;

        using var message = PrepareHttpRequestMessage(context, options, metadata);

        if (message is null)
        {
            return new ReadonlySessionState();
        }

        using var client = _clientFactory.CreateClient();

        using var response = await client.SendAsync(message, context.RequestAborted);

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();

        return await _serializer.DeserializeSessionStateAsync(stream) ?? throw new InvalidOperationException("No session was found.");
    }

    private static HttpRequestMessage? PrepareHttpRequestMessage(HttpContextCore context, RemoteAppSessionStateOptions options, ISessionMetadata metadata)
    {
        var cookie = GetAspNetCookie(context, options);

        if (cookie is null && metadata.IsReadOnly)
        {
            return null;
        }

        var message = new HttpRequestMessage(HttpMethod.Get, options.RemoteAppUrl);

        if (cookie is not null)
        {
            SetAspNetCookie(message, options, cookie);
        }

        SetReadOnly(message, metadata);
        SetApiKey(message, options);

        return message;
    }

    private static void SetReadOnly(HttpRequestMessage message, ISessionMetadata metadata)
        => message.Headers.TryAddWithoutValidation(RemoteAppSessionStateOptions.ReadOnlyHeaderName, metadata.IsReadOnly.ToString());

    private static void SetApiKey(HttpRequestMessage message, RemoteAppSessionStateOptions options)
        => message.Headers.TryAddWithoutValidation(options.ApiKeyHeader, options.ApiKey);

    private static string? GetAspNetCookie(HttpContextCore context, RemoteAppSessionStateOptions options)
        => context.Request.Cookies.TryGetValue(options.CookieName, out var cookie) ? cookie : null;

    private static void SetAspNetCookie(HttpRequestMessage message, RemoteAppSessionStateOptions options, string cookie)
        => message.Headers.TryAddWithoutValidation(HeaderNames.Cookie, $"{options.CookieName}={cookie}");

    private class ReadonlySessionState : ISessionState
    {
        private readonly Dictionary<string, object?> _state = new();

        public object? this[string name]
        {
            get => _state.TryGetValue(name, out var existing) ? existing : null;
            set => _state[name] = value;
        }

        public string SessionID => Guid.NewGuid().ToString();

        public int Count => _state.Count;

        public bool IsReadOnly => true;

        public int TimeOut { get; set; } = 0;

        public bool IsNewSession => true;

        public void Abandon()
        {
        }

        public void Add(string name, object value) => _state[name] = value;

        public void Clear() => _state.Clear();

        public ValueTask DisposeAsync() => default;

        public void Remove(string name) => _state.Remove(name);
    }
}
