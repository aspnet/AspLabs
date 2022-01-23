// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf;
using Type = System.Type;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Tests.Converter
{
    public class JsonConverterFactoryForMessage : JsonConverterFactory
    {
        private readonly JsonSettings _settings;

        public JsonConverterFactoryForMessage(JsonSettings settings)
        {
            _settings = settings;
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(IMessage).IsAssignableFrom(typeToConvert);
        }

        public override JsonConverter CreateConverter(
            Type typeToConvert, JsonSerializerOptions options)
        {
            JsonConverter converter = (JsonConverter)Activator.CreateInstance(
                typeof(MessageConverter<>).MakeGenericType(new Type[] { typeToConvert }),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] { _settings },
                culture: null)!;

            return converter;
        }
    }
}
