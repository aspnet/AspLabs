// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Text.Json;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using HttpApi;
using Type = System.Type;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Tests.Converter
{
    public sealed class AnyConverter : WellKnownTypeConverter
    {
        internal const string AnyTypeUrlField = "@type";
        internal const string AnyWellKnownTypeValueField = "value";

        private readonly JsonSettings _settings;
        private readonly JsonSerializerOptions _options;

        public AnyConverter(JsonSettings settings, JsonSerializerOptions options)
        {
            _settings = settings;
            _options = options;
        }

        protected override string WellKnownTypeName => Any.Descriptor.FullName;

        public sealed override bool CanConvert(Type typeToConvert)
        {
            if (!typeof(IMessage).IsAssignableFrom(typeToConvert))
            {
                return false;
            }

            var property = typeToConvert.GetProperty("Descriptor", BindingFlags.Static | BindingFlags.Public, binder: null, typeof(MessageDescriptor), Type.EmptyTypes, modifiers: null);
            if (property == null)
            {
                return false;
            }

            var descriptor = property.GetValue(null, null) as MessageDescriptor;
            if (descriptor == null)
            {
                return false;
            }

            if (descriptor.FullName != Any.Descriptor.FullName)
            {
                return false;
            }

            return true;
        }

        public override IMessage? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var d = JsonDocument.ParseValue(ref reader);
            if (!d.RootElement.TryGetProperty(AnyTypeUrlField, out var urlField))
            {
                throw new InvalidOperationException("Any message with no @type");
            }

            IMessage message = new Any();
            var typeUrl = urlField.GetString();
            string typeName = Any.GetTypeName(typeUrl);

            MessageDescriptor descriptor = _settings.TypeRegistry.Find(typeName);
            if (descriptor == null)
            {
                throw new InvalidOperationException($"Type registry has no descriptor for type name '{typeName}'");
            }

            IMessage data;
            if (ConverterHelpers.IsWellKnownType(descriptor))
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

        public override void Write(Utf8JsonWriter writer, IMessage value, JsonSerializerOptions options)
        {
            string typeUrl = (string)value.Descriptor.Fields[Any.TypeUrlFieldNumber].Accessor.GetValue(value);
            ByteString data = (ByteString)value.Descriptor.Fields[Any.ValueFieldNumber].Accessor.GetValue(value);
            string typeName = Any.GetTypeName(typeUrl);
            MessageDescriptor descriptor = _settings.TypeRegistry.Find(typeName);
            if (descriptor == null)
            {
                throw new InvalidOperationException($"Type registry has no descriptor for type name '{typeName}'");
            }
            IMessage message = descriptor.Parser.ParseFrom(data);
            writer.WriteStartObject();
            writer.WriteString(AnyTypeUrlField, typeUrl);

            if (ConverterHelpers.IsWellKnownType(descriptor))
            {
                writer.WritePropertyName(AnyWellKnownTypeValueField);
                JsonSerializer.Serialize(writer, message, message.GetType(), _options);
            }
            else
            {
                MessageConverter<Any>.WriteMessageFields(writer, message, _settings, _options);
            }

            writer.WriteEndObject();
        }
    }
}
