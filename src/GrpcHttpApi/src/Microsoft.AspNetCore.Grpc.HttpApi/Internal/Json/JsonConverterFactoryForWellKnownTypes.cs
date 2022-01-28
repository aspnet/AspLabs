// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Type = System.Type;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Internal.Json
{
    public class JsonConverterFactoryForWellKnownTypes : JsonConverterFactory
    {
        private readonly JsonSettings _settings;

        public JsonConverterFactoryForWellKnownTypes(JsonSettings settings)
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

            if (!ConverterHelpers.IsWellKnownType(descriptor))
            {
                return false;
            }

            if (ConverterHelpers.IsWrapperType(descriptor))
            {
                return false;
            }

            return true;
        }

        public override JsonConverter CreateConverter(
            Type typeToConvert, JsonSerializerOptions options)
        {
            var descriptor = JsonConverterHelper.GetMessageDescriptor(typeToConvert)!;
            var converterType = WellKnownTypeNames[descriptor.FullName];

            var converter = (JsonConverter)Activator.CreateInstance(
                converterType.MakeGenericType(new Type[] { typeToConvert }),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] { _settings },
                culture: null)!;

            return converter;
        }

        private static readonly Dictionary<string, Type> WellKnownTypeNames = new Dictionary<string, Type>
        {
            [Timestamp.Descriptor.FullName] = typeof(TimestampConverter<>),
            [ListValue.Descriptor.FullName] = typeof(ListValueConverter<>),
            [Struct.Descriptor.FullName] = typeof(StructConverter<>),
            [Any.Descriptor.FullName] = typeof(AnyConverter<>),
            [Value.Descriptor.FullName] = typeof(ValueConverter<>),
            [Duration.Descriptor.FullName] = typeof(DurationConverter<>),
            //"google/protobuf/any.proto",
            //"google/protobuf/api.proto",
            //"google/protobuf/duration.proto",
            //"google/protobuf/empty.proto",
            //"google/protobuf/wrappers.proto",
            //"google/protobuf/timestamp.proto",
            //"google/protobuf/field_mask.proto",
            //"google/protobuf/source_context.proto",
            //"google/protobuf/struct.proto",
            //"google/protobuf/type.proto",
        };
    }
}
