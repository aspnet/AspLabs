// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Type = System.Type;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Internal.Json
{
    internal sealed class AnyConverter<TMessage> : JsonConverter<TMessage> where TMessage : IMessage, new()
    {
        internal const string AnyTypeUrlField = "@type";
        internal const string AnyWellKnownTypeValueField = "value";

        private readonly JsonSettings _settings;

        public AnyConverter(JsonSettings settings)
        {
            _settings = settings;
        }

        public override TMessage? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var d = JsonDocument.ParseValue(ref reader);
            if (!d.RootElement.TryGetProperty(AnyTypeUrlField, out var urlField))
            {
                throw new InvalidOperationException("Any message with no @type");
            }

            var message = new TMessage();
            var typeUrl = urlField.GetString();
            var typeName = Any.GetTypeName(typeUrl);

            var descriptor = _settings.TypeRegistry.Find(typeName);
            if (descriptor == null)
            {
                throw new InvalidOperationException($"Type registry has no descriptor for type name '{typeName}'");
            }

            IMessage data;
            if (JsonConverterHelper.IsWellKnownType(descriptor))
            {
                if (!d.RootElement.TryGetProperty(AnyWellKnownTypeValueField, out var valueField))
                {
                    throw new InvalidOperationException($"Expected '{AnyWellKnownTypeValueField}' property for well-known type Any body");
                }

                data = (IMessage)JsonSerializer.Deserialize(valueField, descriptor.ClrType, options)!;
            }
            else
            {
                data = (IMessage)JsonSerializer.Deserialize(d.RootElement, descriptor.ClrType, options)!;
            }

            message.Descriptor.Fields[Any.TypeUrlFieldNumber].Accessor.SetValue(message, typeUrl);
            message.Descriptor.Fields[Any.ValueFieldNumber].Accessor.SetValue(message, data.ToByteString());

            return message;
        }

        public override void Write(Utf8JsonWriter writer, TMessage value, JsonSerializerOptions options)
        {
            var typeUrl = (string)value.Descriptor.Fields[Any.TypeUrlFieldNumber].Accessor.GetValue(value);
            var data = (ByteString)value.Descriptor.Fields[Any.ValueFieldNumber].Accessor.GetValue(value);
            var typeName = Any.GetTypeName(typeUrl);
            var descriptor = _settings.TypeRegistry.Find(typeName);
            if (descriptor == null)
            {
                throw new InvalidOperationException($"Type registry has no descriptor for type name '{typeName}'");
            }
            var valueMessage = descriptor.Parser.ParseFrom(data);
            writer.WriteStartObject();
            writer.WriteString(AnyTypeUrlField, typeUrl);

            if (JsonConverterHelper.IsWellKnownType(descriptor))
            {
                writer.WritePropertyName(AnyWellKnownTypeValueField);
                if (JsonConverterHelper.IsWrapperType(descriptor))
                {
                    var wrappedValue = valueMessage.Descriptor.Fields[JsonConverterHelper.WrapperValueFieldNumber].Accessor.GetValue(valueMessage);
                    JsonSerializer.Serialize(writer, wrappedValue, wrappedValue.GetType(), options);
                }
                else
                {
                    JsonSerializer.Serialize(writer, valueMessage, valueMessage.GetType(), options);
                }
            }
            else
            {
                MessageConverter<Any>.WriteMessageFields(writer, valueMessage, _settings, options);
            }

            writer.WriteEndObject();
        }
    }
}
