// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

#if NETCOREAPP3_1_OR_GREATER
using System.Web.Adapters;
using Microsoft.Extensions.Options;
#endif

#if NET472
using System.Web.SessionState;
#endif

namespace System.Web.Adapters.SessionState;

internal partial class SessionSerializer
{
#if NETCOREAPP3_1_OR_GREATER
    public SessionSerializer(IOptions<RemoteAppSessionStateOptions> options)
        : this(options.Value.KnownKeys)
    {
    }
#endif

    public SessionSerializer(IDictionary<string, Type> map)
    {
        Options = new JsonSerializerOptions
        {
#if !NETCOREAPP3_1
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
#endif
            AllowTrailingCommas = true,
            IgnoreReadOnlyProperties = true,
            Converters =
            {
                new SerializedSessionConverter(map),
            }
        };
    }

    public JsonSerializerOptions Options { get; }

    public RemoteSessionData? Deserialize(string? jsonString)
        => jsonString?.Length > 0
        ? JsonSerializer.Deserialize<RemoteSessionData>(jsonString, Options)
        : null;

    public async ValueTask<RemoteSessionData?> DeserializeAsync(Stream stream)
        => stream?.Length > 0
        ? await JsonSerializer.DeserializeAsync<RemoteSessionData>(stream, Options)
        : null;

    public async ValueTask SerializeAsync(RemoteSessionData remoteSessionState, Stream stream, CancellationToken token)
    {
        await JsonSerializer.SerializeAsync(stream, remoteSessionState, Options, token);
    }

#if NET472
    public async ValueTask SerializeAsync(HttpSessionState state, Stream stream, CancellationToken token)
    {
        if (state is null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        var values = new SessionValues();

        foreach (string key in state.Keys)
        {
            values.Add(key, state[key]);
        }

        var session = new RemoteSessionData
        {
            IsNewSession = state.IsNewSession,
            IsReadOnly = state.IsReadOnly,
            SessionID = state.SessionID,
            Timeout = state.Timeout,
            Values = values
        };

        await JsonSerializer.SerializeAsync(stream, session, Options, token);
    }
#endif

    private class SerializedSessionConverter : JsonConverter<SessionValues>
    {
        private readonly IDictionary<string, Type> _map;

        public SerializedSessionConverter(IDictionary<string, Type> map)
        {
            _map = map;
        }

#pragma warning disable CS8764 // Nullability of return type doesn't match overridden member (possibly because of nullability attributes).
        public override SessionValues? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
#pragma warning restore CS8764 // Nullability of return type doesn't match overridden member (possibly because of nullability attributes).
        {
            var state = new SessionValues();

            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType is not JsonTokenType.PropertyName || reader.GetString() is not { } key)
                {
                    throw new InvalidOperationException("Key entry must be a string");
                }

                if (!_map.TryGetValue(key, out var type))
                {
                    throw new InvalidOperationException($"Key '{key}' is not registered");
                }

                if (!reader.Read())
                {
                    throw new InvalidOperationException();
                }

                state.Add(key, JsonSerializer.Deserialize(ref reader, type, options));
            }

            return state;
        }

        public override void Write(Utf8JsonWriter writer, SessionValues session, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (var key in session.Keys)
            {
                writer.WritePropertyName(key);

                if (session[key] is { } value)
                {
                    if (!_map.TryGetValue(key, out var type))
                    {
                        throw new InvalidOperationException($"Key '{key}' is not registered");
                    }

                    JsonSerializer.Serialize(writer, value, type, options);
                }
                else
                {
                    writer.WriteNullValue();
                }
            }

            writer.WriteEndObject();
        }
    }
}
