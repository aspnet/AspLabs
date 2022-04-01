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
    internal sealed class ListValueConverter<TMessage> : JsonConverter<TMessage> where TMessage : IMessage, new()
    {
        public ListValueConverter(JsonSettings settings)
        {
        }

        public override TMessage? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var message = new TMessage();
            JsonConverterHelper.PopulateList(ref reader, options, message, message.Descriptor.Fields[ListValue.ValuesFieldNumber]);

            return message;
        }

        public override void Write(Utf8JsonWriter writer, TMessage value, JsonSerializerOptions options)
        {
            var list = (IList)value.Descriptor.Fields[ListValue.ValuesFieldNumber].Accessor.GetValue(value);

            JsonSerializer.Serialize(writer, list, list.GetType(), options);
        }
    }
}
