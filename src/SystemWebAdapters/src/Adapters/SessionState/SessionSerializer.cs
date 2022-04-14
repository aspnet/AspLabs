// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Collections;

#if NETCOREAPP3_1_OR_GREATER
using System.Web.Adapters;
using Microsoft.Extensions.Options;
#endif

#if NET472
using System.Web.SessionState;
#endif

namespace System.Web.Adapters.SessionState;

internal class SessionSerializer
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
            IgnoreReadOnlyProperties = true,
            Converters =
            {
                new SerializedSessionConverter(map),
            }
        };
    }

    public JsonSerializerOptions Options { get; }

#if NETCOREAPP3_1_OR_GREATER
    public async ValueTask<ISessionState?> DeserializeAsync(Stream stream)
        => await JsonSerializer.DeserializeAsync<SessionState>(stream, Options);

    public async ValueTask SerializeAsync(ISessionState sessionState, Stream stream, CancellationToken token)
    {
        var session = (SessionState)sessionState;

        await JsonSerializer.SerializeAsync(stream, session, Options, token);
    }
#endif

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

        var session = new SessionState
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

                if (JsonSerializer.Deserialize(ref reader, type, options) is { } result)
                {
                    state.Add(key, result);
                }
            }

            return state;
        }

        public override void Write(Utf8JsonWriter writer, SessionValues session, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (var (key, value) in session.KeyValues)
            {
                writer.WritePropertyName(key);

                if (value is null)
                {
                    writer.WriteNullValue();
                }
                else
                {
                    if (!_map.TryGetValue(key, out var type))
                    {
                        throw new InvalidOperationException($"Key '{key}' is not registered");
                    }

                    JsonSerializer.Serialize(writer, value, type, options);
                }
            }

            writer.WriteEndObject();
        }
    }

    private class SessionState
#if NETCOREAPP3_1_OR_GREATER
        : ISessionState
#endif
    {
        public object? this[string name]
        {
            get => Values[name];
            set => Values[name] = value;
        }

        public string SessionID { get; set; } = null!;

        public bool IsReadOnly { get; set; }

        public SessionValues Values { get; set; } = null!;

        public int Count => Values.Count;

        public int Timeout { get; set; }

        public bool IsNewSession { get; set; }

        public bool IsAbandoned { get; set; }

        public bool IsSynchronized => ((ICollection)Values).IsSynchronized;

        public object SyncRoot => ((ICollection)Values).SyncRoot;

        public void Abandon() => IsAbandoned = true;

        public void Add(string name, object value) => Values.Add(name, value);

        public void Clear() => Values.Clear();

        public void Remove(string name) => Values.Remove(name);

        public ValueTask CommitAsync(CancellationToken token) => default;

        public void Dispose()
        {
        }

        public void CopyTo(Array array, int index) => ((ICollection)Values).CopyTo(array, index);

        public IEnumerator GetEnumerator() => Values.GetEnumerator();
    }

    private class SessionValues : NameObjectCollectionBase
    {
        public void Add(string key, object value) => BaseAdd(key, value);

        public void Clear() => BaseClear();

        public void Remove(string key) => BaseRemove(key);

        public IEnumerable<(string, object?)> KeyValues
        {
            get
            {
                foreach (string? key in Keys)
                {
                    if (key is not null)
                    {
                        yield return (key, BaseGet(key));
                    }
                }
            }
        }

        public object? this[string key]
        {
            get => BaseGet(key);
            set => BaseSet(key, value);
        }
    }
}
