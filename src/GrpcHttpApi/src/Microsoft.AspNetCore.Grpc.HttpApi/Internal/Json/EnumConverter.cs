// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf.Reflection;
using Type = System.Type;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Internal.Json
{
    public sealed class EnumConverter : JsonConverter<Enum>
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

        public override Enum? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    var enumDescriptor = ResolveEnumDescriptor(typeToConvert);
                    if (enumDescriptor == null)
                    {
                        throw new InvalidOperationException($"Unable to resolve descriptor for {typeToConvert}.");
                    }
                    var valueDescriptor = enumDescriptor.FindValueByName(reader.GetString()!);
                    return (Enum)Enum.ToObject(typeToConvert, valueDescriptor.Number);
                case JsonTokenType.Number:
                    return (Enum)Enum.ToObject(typeToConvert, reader.GetInt32());
                case JsonTokenType.Null:
                    return null;
                default:
                    throw new InvalidOperationException($"Unexpected JSON token: {reader.TokenType}");
            }
        }

        private static EnumDescriptor? ResolveEnumDescriptor(Type typeToConvert)
        {
            var containingType = typeToConvert?.DeclaringType?.DeclaringType;

            if (containingType != null)
            {
                var messageDescriptor = JsonConverterHelper.GetMessageDescriptor(containingType);
                if (messageDescriptor != null)
                {
                    for (var i = 0; i < messageDescriptor.EnumTypes.Count; i++)
                    {
                        if (messageDescriptor.EnumTypes[i].ClrType == typeToConvert)
                        {
                            return messageDescriptor.EnumTypes[i];
                        }
                    }
                }
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, Enum value, JsonSerializerOptions options)
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
