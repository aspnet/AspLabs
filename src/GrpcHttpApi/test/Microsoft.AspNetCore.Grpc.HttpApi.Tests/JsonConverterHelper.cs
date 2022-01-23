// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.Json;
using Google.Protobuf.Reflection;
using Microsoft.AspNetCore.Grpc.HttpApi.Tests.Converter;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Tests
{
    internal static class JsonConverterHelper
    {
        internal static JsonSerializerOptions CreateSerializerOptions(JsonSettings? settings, TypeRegistry typeRegistery)
        {
            var resolvedSettings = settings ?? new JsonSettings { TypeRegistry = typeRegistery };
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals
            };
            var converter = new JsonConverterFactoryForMessage(resolvedSettings);
            jsonSerializerOptions.Converters.Add(new AnyConverter(resolvedSettings));
            jsonSerializerOptions.Converters.Add(new TimestampConverter());
            jsonSerializerOptions.Converters.Add(converter);
            jsonSerializerOptions.Converters.Add(new ByteStringConverter());
            jsonSerializerOptions.Converters.Add(new Int64Converter());
            jsonSerializerOptions.Converters.Add(new UInt64Converter());
            jsonSerializerOptions.Converters.Add(new EnumConverter(resolvedSettings));
            jsonSerializerOptions.Converters.Add(new BoolConverter());
            return jsonSerializerOptions;
        }
    }
}
