// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Type = System.Type;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Tests.Converter
{
    public sealed class EnumConverter : JsonConverter<System.Enum>
    {
        private readonly JsonSettings _settings;

        public EnumConverter(JsonSettings settings)
        {
            _settings = settings;
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsEnum;
        }

        public override System.Enum? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, System.Enum value, JsonSerializerOptions options)
        {
            if (_settings.FormatEnumsAsIntegers)
            {
                writer.WriteNumberValue((int)(object)value);
            }
            else
            {
                var name = OriginalEnumValueHelper.GetOriginalName(value);
                if (name != null)
                {
                    writer.WriteStringValue(name);
                }
                else
                {
                    writer.WriteNumberValue((int)(object)value);
                }
            }
        }
    }
}
