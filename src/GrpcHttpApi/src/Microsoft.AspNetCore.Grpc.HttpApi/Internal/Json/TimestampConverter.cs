// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Type = System.Type;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Internal.Json
{
    internal sealed class TimestampConverter<TMessage> : JsonConverter<TMessage> where TMessage : IMessage, new()
    {
        public TimestampConverter(JsonSettings settings)
        {
        }

        public override TMessage? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new InvalidOperationException("Expected string value for Timestamp");
            }
            var (seconds, nanos) = Legacy.ParseTimestamp(reader.GetString()!);

            var message = new TMessage();
            if (message is Timestamp timestamp)
            {
                timestamp.Seconds = seconds;
                timestamp.Nanos = nanos;
            }
            else
            {
                message.Descriptor.Fields[Timestamp.SecondsFieldNumber].Accessor.SetValue(message, seconds);
                message.Descriptor.Fields[Timestamp.NanosFieldNumber].Accessor.SetValue(message, nanos);
            }
            return message;
        }

        public override void Write(Utf8JsonWriter writer, TMessage value, JsonSerializerOptions options)
        {
            int nanos;
            long seconds;
            if (value is Timestamp timestamp)
            {
                nanos = timestamp.Nanos;
                seconds = timestamp.Seconds;
            }
            else
            {
                nanos = (int)value.Descriptor.Fields[Timestamp.NanosFieldNumber].Accessor.GetValue(value);
                seconds = (long)value.Descriptor.Fields[Timestamp.SecondsFieldNumber].Accessor.GetValue(value);
            }

            var text = Legacy.GetTimestampText(nanos, seconds);
            writer.WriteStringValue(text);
        }
    }
}
