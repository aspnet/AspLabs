// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf;
using Type = System.Type;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Tests.Converter
{
    public sealed class ByteStringConverter : JsonConverter<ByteString>
    {
        public override ByteString? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return UnsafeByteOperations.UnsafeWrap(reader.GetBytesFromBase64());
        }

        public override void Write(Utf8JsonWriter writer, ByteString value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToBase64());
        }
    }
}
