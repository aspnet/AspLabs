// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Type = System.Type;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Internal.Json
{
    public sealed class StructConverter<TMessage> : JsonConverter<TMessage> where TMessage : IMessage, new()
    {
        public StructConverter(JsonSettings settings)
        {
        }

        public override TMessage? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var message = new TMessage();
            JsonConverterHelper.PopulateMap(ref reader, options, message, message.Descriptor.Fields[Struct.FieldsFieldNumber]);

            return message;
        }

        public override void Write(Utf8JsonWriter writer, TMessage value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            var fields = (IDictionary)value.Descriptor.Fields[Struct.FieldsFieldNumber].Accessor.GetValue(value);
            foreach (DictionaryEntry entry in fields)
            {
                var k = (string)entry.Key;
                var v = (IMessage?)entry.Value;
                if (string.IsNullOrEmpty(k) || v == null)
                {
                    throw new InvalidOperationException("Struct fields cannot have an empty key or a null value.");
                }

                writer.WritePropertyName(k);
                JsonSerializer.Serialize(writer, v, v.GetType(), options);
            }

            writer.WriteEndObject();
        }
    }
}
