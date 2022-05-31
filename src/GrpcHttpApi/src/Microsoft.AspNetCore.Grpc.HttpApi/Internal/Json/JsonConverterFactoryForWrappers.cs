// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf;
using Type = System.Type;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Internal.Json
{
    internal class JsonConverterFactoryForWrappers : JsonConverterFactory
    {
        private readonly JsonSettings _settings;

        public JsonConverterFactoryForWrappers(JsonSettings settings)
        {
            _settings = settings;
        }

        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeof(IMessage).IsAssignableFrom(typeToConvert))
            {
                return false;
            }

            var descriptor = JsonConverterHelper.GetMessageDescriptor(typeToConvert);
            if (descriptor == null)
            {
                return false;
            }

            return JsonConverterHelper.IsWrapperType(descriptor);
        }

        public override JsonConverter CreateConverter(
            Type typeToConvert, JsonSerializerOptions options)
        {
            var converter = (JsonConverter)Activator.CreateInstance(
                typeof(WrapperConverter<>).MakeGenericType(new Type[] { typeToConvert }),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] { _settings },
                culture: null)!;

            return converter;
        }
    }
}
