// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Type = System.Type;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Tests.Converter
{
    public sealed class MessageConverter<TMessage> : JsonConverter<TMessage> where TMessage : IMessage, new()
    {
        private readonly JsonSettings _settings;

        public MessageConverter(JsonSettings settings)
        {
            _settings = settings;
        }

        public override TMessage Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new Exception();
            }

            var message = new TMessage();
            var jsonFieldMap = CreateJsonFieldMap(message.Descriptor.Fields.InFieldNumberOrder());

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.EndObject:
                        return message;
                    case JsonTokenType.PropertyName:
                        if (jsonFieldMap.TryGetValue(reader.GetString()!, out var fieldDescriptor))
                        {
                            if (fieldDescriptor.ContainingOneof != null)
                            {
                                if (fieldDescriptor.ContainingOneof.Accessor.GetCaseFieldDescriptor(message) != null)
                                {
                                    throw new InvalidOperationException($"Multiple values specified for oneof {fieldDescriptor.ContainingOneof.Name}");
                                }
                            }

                            if (fieldDescriptor.IsMap)
                            {
                                var mapFields = fieldDescriptor.MessageType.Fields.InFieldNumberOrder();
                                var mapKey = mapFields[0];
                                var mapValue = mapFields[1];

                                var keyType = GetFieldType(mapKey);
                                var valueType = GetFieldType(mapValue);

                                var repeatedFieldType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
                                var newValues = (IDictionary)JsonSerializer.Deserialize(ref reader, repeatedFieldType, options)!;

                                var existingValue = (IDictionary)fieldDescriptor.Accessor.GetValue(message);
                                foreach (DictionaryEntry item in newValues)
                                {
                                    existingValue[item.Key] = item.Value;
                                }
                            }
                            else if (fieldDescriptor.IsRepeated)
                            {
                                var fieldType = GetFieldType(fieldDescriptor);
                                var repeatedFieldType = typeof(List<>).MakeGenericType(fieldType);
                                var newValues = (IList)JsonSerializer.Deserialize(ref reader, repeatedFieldType, options)!;

                                var existingValue = (IList)fieldDescriptor.Accessor.GetValue(message);
                                foreach (var item in newValues)
                                {
                                    existingValue.Add(item);
                                }
                            }
                            else
                            {
                                var fieldType = GetFieldType(fieldDescriptor);
                                var propertyValue = JsonSerializer.Deserialize(ref reader, fieldType, options);
                                fieldDescriptor.Accessor.SetValue(message, propertyValue);
                            }
                        }
                        else
                        {
                            reader.Skip();
                        }
                        break;
                    case JsonTokenType.Comment:
                        // Ignore
                        break;
                    default:
                        throw new InvalidOperationException($"Unexpected JSON token: {reader.TokenType}");
                }
            }

            throw new Exception();
        }

        public override void Write(
            Utf8JsonWriter writer,
            TMessage value,
            JsonSerializerOptions options)
        {
            WriteMessage(writer, value, options);
        }

        private void WriteMessage(Utf8JsonWriter writer, IMessage message, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            WriteMessageFields(writer, message, _settings, options);

            writer.WriteEndObject();
        }

        internal static void WriteMessageFields(Utf8JsonWriter writer, IMessage message, JsonSettings settings, JsonSerializerOptions options)
        {
            var fields = message.Descriptor.Fields;

            foreach (var field in fields.InFieldNumberOrder())
            {
                var accessor = field.Accessor;
                var value = accessor.GetValue(message);
                if (!ShouldFormatFieldValue(message, field, value, settings.FormatDefaultValues))
                {
                    continue;
                }

                writer.WritePropertyName(accessor.Descriptor.JsonName);
                JsonSerializer.Serialize(writer, value, value.GetType(), options);
            }
        }

        private static Dictionary<string, FieldDescriptor> CreateJsonFieldMap(IList<FieldDescriptor> fields)
        {
            var map = new Dictionary<string, FieldDescriptor>();
            foreach (var field in fields)
            {
                map[field.Name] = field;
                map[field.JsonName] = field;
            }
            return new Dictionary<string, FieldDescriptor>(map);
        }

        /// <summary>
        /// Determines whether or not a field value should be serialized according to the field,
        /// its value in the message, and the settings of this formatter.
        /// </summary>
        private static bool ShouldFormatFieldValue(IMessage message, FieldDescriptor field, object value, bool formatDefaultValues) =>
            field.HasPresence
            // Fields that support presence *just* use that
            ? field.Accessor.HasValue(message)
            // Otherwise, format if either we've been asked to format default values, or if it's
            // not a default value anyway.
            : formatDefaultValues || !IsDefaultValue(field, value);

        private static bool IsDefaultValue(FieldDescriptor descriptor, object value)
        {
            if (descriptor.IsMap)
            {
                IDictionary dictionary = (IDictionary)value;
                return dictionary.Count == 0;
            }
            if (descriptor.IsRepeated)
            {
                IList list = (IList)value;
                return list.Count == 0;
            }
            switch (descriptor.FieldType)
            {
                case FieldType.Bool:
                    return (bool)value == false;
                case FieldType.Bytes:
                    return (ByteString)value == ByteString.Empty;
                case FieldType.String:
                    return (string)value == "";
                case FieldType.Double:
                    return (double)value == 0.0;
                case FieldType.SInt32:
                case FieldType.Int32:
                case FieldType.SFixed32:
                case FieldType.Enum:
                    return (int)value == 0;
                case FieldType.Fixed32:
                case FieldType.UInt32:
                    return (uint)value == 0;
                case FieldType.Fixed64:
                case FieldType.UInt64:
                    return (ulong)value == 0;
                case FieldType.SFixed64:
                case FieldType.Int64:
                case FieldType.SInt64:
                    return (long)value == 0;
                case FieldType.Float:
                    return (float)value == 0f;
                case FieldType.Message:
                case FieldType.Group: // Never expect to get this, but...
                    return value == null;
                default:
                    throw new ArgumentException("Invalid field type");
            }
        }

        private static Type GetFieldType(FieldDescriptor descriptor)
        {
            switch (descriptor.FieldType)
            {
                case FieldType.Bool:
                    return typeof(bool);
                case FieldType.Bytes:
                    return typeof(ByteString);
                case FieldType.String:
                    return typeof(string);
                case FieldType.Double:
                    return typeof(double);
                case FieldType.SInt32:
                case FieldType.Int32:
                case FieldType.SFixed32:
                case FieldType.Enum:
                    return typeof(int);
                case FieldType.Fixed32:
                case FieldType.UInt32:
                    return typeof(uint);
                case FieldType.Fixed64:
                case FieldType.UInt64:
                    return typeof(ulong);
                case FieldType.SFixed64:
                case FieldType.Int64:
                case FieldType.SInt64:
                    return typeof(long);
                case FieldType.Float:
                    return typeof(float);
                case FieldType.Message:
                case FieldType.Group: // Never expect to get this, but...
                    if (ConverterHelpers.IsWrapperType(descriptor.MessageType))
                    {
                        var t = GetFieldType(descriptor.MessageType.Fields[ConverterHelpers.WrapperValueFieldNumber]);
                        if (t.IsValueType)
                        {
                            return typeof(Nullable<>).MakeGenericType(t);
                        }

                        return t;
                    }
                    return descriptor.MessageType.ClrType;
                default:
                    throw new ArgumentException("Invalid field type");
            }
        }
    }
}
