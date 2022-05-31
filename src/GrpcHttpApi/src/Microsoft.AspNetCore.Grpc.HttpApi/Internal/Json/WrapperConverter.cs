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
    internal sealed class WrapperConverter<TMessage> : JsonConverter<TMessage> where TMessage : IMessage, new()
    {
        public WrapperConverter(JsonSettings settings)
        {
        }

        public override TMessage? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var message = new TMessage();
            var valueDescriptor = message.Descriptor.Fields[JsonConverterHelper.WrapperValueFieldNumber];
            var t = JsonConverterHelper.GetFieldType(valueDescriptor);
            var value = JsonSerializer.Deserialize(ref reader, t, options);
            valueDescriptor.Accessor.SetValue(message, value);

            return message;
        }

        public override void Write(Utf8JsonWriter writer, TMessage value, JsonSerializerOptions options)
        {
            var valueDescriptor = value.Descriptor.Fields[JsonConverterHelper.WrapperValueFieldNumber];
            var innerValue = valueDescriptor.Accessor.GetValue(value);
            JsonSerializer.Serialize(writer, innerValue, innerValue.GetType(), options);
        }
    }
}
