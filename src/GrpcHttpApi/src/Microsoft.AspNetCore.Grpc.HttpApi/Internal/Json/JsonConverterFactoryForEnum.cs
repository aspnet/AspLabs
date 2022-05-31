// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Type = System.Type;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Internal.Json
{
    internal class JsonConverterFactoryForEnum : JsonConverterFactory
    {
        private readonly JsonSettings _settings;

        public JsonConverterFactoryForEnum(JsonSettings settings)
        {
            _settings = settings;
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsEnum;
        }

        public override JsonConverter CreateConverter(
            Type typeToConvert, JsonSerializerOptions options)
        {
            var converter = (JsonConverter)Activator.CreateInstance(
                typeof(EnumConverter<>).MakeGenericType(new Type[] { typeToConvert }),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] { _settings },
                culture: null)!;

            return converter;
        }
    }
}
