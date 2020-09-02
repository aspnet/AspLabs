// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.DynamicJS
{
    internal class JSObjectJsonConverter : JsonConverter<JSObject>
    {
        private static readonly string _objectIdKey = "__objectId";

        public override JSObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new InvalidOperationException($"{typeof(JSObject)} does not support deserialization.");
        }

        public override void Write(Utf8JsonWriter writer, JSObject value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber(_objectIdKey, value.Id);
            writer.WriteEndObject();
        }
    }
}
