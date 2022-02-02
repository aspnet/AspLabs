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

namespace Microsoft.AspNetCore.Grpc.HttpApi.Internal.Json
{
    internal sealed class MessageConverter<TMessage> : JsonConverter<TMessage> where TMessage : IMessage, new()
    {
        private readonly JsonSettings _settings;
        private readonly Dictionary<string, FieldDescriptor> _jsonFieldMap;

        public MessageConverter(JsonSettings settings)
        {
            _settings = settings;
            _jsonFieldMap = CreateJsonFieldMap((new TMessage()).Descriptor.Fields.InFieldNumberOrder());
        }

        public override TMessage Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var message = new TMessage();

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new InvalidOperationException($"Unexpected JSON token: {reader.TokenType}");
            }

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.EndObject:
                        return message;
                    case JsonTokenType.PropertyName:
                        if (_jsonFieldMap.TryGetValue(reader.GetString()!, out var fieldDescriptor))
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
                                JsonConverterHelper.PopulateMap(ref reader, options, message, fieldDescriptor);
                            }
                            else if (fieldDescriptor.IsRepeated)
                            {
                                JsonConverterHelper.PopulateList(ref reader, options, message, fieldDescriptor);
                            }
                            else
                            {
                                var fieldType = JsonConverterHelper.GetFieldType(fieldDescriptor);
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
    }
}
