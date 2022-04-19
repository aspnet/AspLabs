// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

internal partial class SessionSerializer : ISessionSerializer
{
    private readonly JsonSerializerOptions _options;

    public SessionSerializer(IDictionary<string, Type> map, bool writeIndented = false)
    {
        _options = new JsonSerializerOptions
        {
#if !NETCOREAPP3_1
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
#endif
            AllowTrailingCommas = true,
            WriteIndented = writeIndented,
            Converters =
            {
                new SerializedSessionConverter(map),
            }
        };
    }

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
            SessionValues? values = null;

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
                    if (values is null)
                    {
                        values = new();
                    }

                    values.Add(key, result);
                }
            }

            return values;
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
