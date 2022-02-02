// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf.Reflection;
using Type = System.Type;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Internal.Json
{
    internal sealed class EnumConverter<TEnum> : JsonConverter<TEnum> where TEnum : Enum
    {
        private readonly JsonSettings _settings;

        public EnumConverter(JsonSettings settings)
        {
            _settings = settings;
        }

        public override TEnum? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
                    
                    return ConvertInteger(valueDescriptor.Number);
                case JsonTokenType.Number:
                    return ConvertInteger(reader.GetInt32());
                case JsonTokenType.Null:
                    return default;
                default:
                    throw new InvalidOperationException($"Unexpected JSON token: {reader.TokenType}");
            }
        }

        private static TEnum ConvertInteger(int integer)
        {
            if (!TryConvertToEnum(integer, out var value))
            {
                throw new InvalidOperationException($"Integer can't be converted to enum {value.GetType().FullName}.");
            }

            return value;
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

        public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
        {
            if (_settings.FormatEnumsAsIntegers)
            {
                if (!TryConvertToInteger(value, out var integer))
                {
                    throw new InvalidOperationException($"Enum {value.GetType().FullName} can't be converted to integer.");
                }
                writer.WriteNumberValue(integer);
            }
            else
            {
                var name = Legacy.OriginalEnumValueHelper.GetOriginalName(value);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryConvertToInteger(TEnum value, out int integer)
        {
            if (Unsafe.SizeOf<int>() == Unsafe.SizeOf<TEnum>())
            {
                integer = Unsafe.As<TEnum, int>(ref value);
                return true;
            }
            integer = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryConvertToEnum(int integer, out TEnum value)
        {
            if (Unsafe.SizeOf<int>() == Unsafe.SizeOf<TEnum>())
            {
                value = Unsafe.As<int, TEnum>(ref integer);
                return true;
            }
            value = default;
            return false;
        }
    }
}
