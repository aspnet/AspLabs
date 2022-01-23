// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
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

        /*
        private void WriteValue(Utf8JsonWriter writer, object value)
        {
            if (value == null || value is NullValue)
            {
                writer.WriteNullValue();
            }
            else if (value is bool b)
            {
                writer.WriteBooleanValue(b);
            }
            else if (value is ByteString bs)
            {
                writer.WriteStringValue(bs.ToBase64());
            }
            else if (value is string s)
            {
                writer.WriteStringValue(s);
            }
            else if (value is IDictionary)
            {
                WriteDictionary(writer, (IDictionary)value);
            }
            else if (value is IList)
            {
                WriteList(writer, (IList)value);
            }
            else if (value is int)
            {
                writer.WriteNumberValue((int)value);
            }
            else if (value is uint)
            {
                writer.WriteNumberValue((uint)value);
            }
            else if (value is long || value is ulong)
            {
                IFormattable formattable = (IFormattable)value;
                writer.WriteStringValue(formattable.ToString("d", CultureInfo.InvariantCulture));
            }
            else if (value is System.Enum)
            {
                if (_settings.FormatEnumsAsIntegers)
                {
                    WriteValue(writer, (int)value);
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
                        writer.WriteNumberValue((int)value);
                    }
                }
            }
            else if (value is float)
            {
                var f = (float)value;
                if (float.IsNaN(f))
                {
                    writer.WriteStringValue("NaN");
                }
                else if (float.IsPositiveInfinity(f))
                {
                    writer.WriteStringValue("Infinity");
                }
                else if (float.IsNegativeInfinity(f))
                {
                    writer.WriteStringValue("-Infinity");
                }
                else
                {
                    writer.WriteNumberValue(f);
                }
            }
            else if (value is double)
            {
                var d = (double)value;
                if (double.IsNaN(d))
                {
                    writer.WriteStringValue("NaN");
                }
                else if (double.IsPositiveInfinity(d))
                {
                    writer.WriteStringValue("Infinity");
                }
                else if (double.IsNegativeInfinity(d))
                {
                    writer.WriteStringValue("-Infinity");
                }
                else
                {
                    writer.WriteNumberValue(d);
                }
            }
            else if (value is IMessage)
            {
                JsonSerializer.Serialize(writer, (IMessage)value, _options);
            }
            else
            {
                throw new ArgumentException("Unable to format value of type " + value.GetType());
            }
        }

        private void WriteList(Utf8JsonWriter writer, IList list)
        {
            writer.WriteStartArray();

            foreach (var value in list)
            {
                WriteValue(writer, value);
            }

            writer.WriteEndArray();
        }

        private void WriteDictionary(Utf8JsonWriter writer, IDictionary dictionary)
        {
            writer.WriteStartObject();

            foreach (DictionaryEntry pair in dictionary)
            {
                string keyText;
                if (pair.Key is string)
                {
                    keyText = (string)pair.Key;
                }
                else if (pair.Key is bool)
                {
                    keyText = (bool)pair.Key ? "true" : "false";
                }
                else if (pair.Key is int || pair.Key is uint | pair.Key is long || pair.Key is ulong)
                {
                    keyText = ((IFormattable)pair.Key).ToString("d", CultureInfo.InvariantCulture);
                }
                else
                {
                    if (pair.Key == null)
                    {
                        throw new ArgumentException("Dictionary has entry with null key");
                    }
                    throw new ArgumentException("Unhandled dictionary key type: " + pair.Key.GetType());
                }

                writer.WritePropertyName(keyText);
                WriteValue(writer, pair.Value);
            }

            writer.WriteEndObject();
        }
        */

        /*
        /// <summary>
        /// Returns whether this message is one of the "wrapper types" used for fields which represent primitive values
        /// with the addition of presence.
        /// </summary>
        private bool IsWrapperType(MessageDescriptor descriptor)
        {
            return descriptor.File.Package == "google.protobuf" && descriptor.File.Name == "google/protobuf/wrappers.proto";
        }

        /// <summary>
        /// Field number for the single "value" field in all wrapper types.
        /// </summary>
        internal const int WrapperValueFieldNumber = Int32Value.ValueFieldNumber;

        /// <summary>
        /// Central interception point for well-known type formatting. Any well-known types which
        /// don't need special handling can fall back to WriteMessage. We avoid assuming that the
        /// values are using the embedded well-known types, in order to allow for dynamic messages
        /// in the future.
        /// </summary>
        private void WriteWellKnownTypeValue(Utf8JsonWriter writer, MessageDescriptor descriptor, object value)
        {
            // Currently, we can never actually get here, because null values are always handled by the caller. But if we *could*,
            // this would do the right thing.
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            // For wrapper types, the value will either be the (possibly boxed) "native" value,
            // or the message itself if we're formatting it at the top level (e.g. just calling ToString on the object itself).
            // If it's the message form, we can extract the value first, which *will* be the (possibly boxed) native value,
            // and then proceed, writing it as if we were definitely in a field. (We never need to wrap it in an extra string...
            // WriteValue will do the right thing.)
            if (IsWrapperType(descriptor))
            {
                if (value is IMessage)
                {
                    var message = (IMessage)value;
                    value = message.Descriptor.Fields[WrapperValueFieldNumber].Accessor.GetValue(message);
                }
                WriteValue(writer, value);
                return;
            }
            if (descriptor.FullName == Timestamp.Descriptor.FullName)
            {
                WriteTimestamp(writer, (IMessage)value);
                return;
            }
            if (descriptor.FullName == Duration.Descriptor.FullName)
            {
                WriteDuration(writer, (IMessage)value);
                return;
            }
            if (descriptor.FullName == FieldMask.Descriptor.FullName)
            {
                WriteFieldMask(writer, (IMessage)value);
                return;
            }
            if (descriptor.FullName == Struct.Descriptor.FullName)
            {
                WriteStruct(writer, (IMessage)value);
                return;
            }
            if (descriptor.FullName == ListValue.Descriptor.FullName)
            {
                var fieldAccessor = descriptor.Fields[ListValue.ValuesFieldNumber].Accessor;
                WriteList(writer, (IList)fieldAccessor.GetValue((IMessage)value));
                return;
            }
            if (descriptor.FullName == Value.Descriptor.FullName)
            {
                WriteStructFieldValue(writer, (IMessage)value);
                return;
            }
            if (descriptor.FullName == Any.Descriptor.FullName)
            {
                WriteAny(writer, (IMessage)value);
                return;
            }
            WriteMessage(writer, (IMessage)value);
        }

        private void WriteTimestamp(Utf8JsonWriter writer, IMessage value)
        {
            // TODO: In the common case where this *is* using the built-in Timestamp type, we could
            // avoid all the reflection at this point, by casting to Timestamp. In the interests of
            // avoiding subtle bugs, don't do that until we've implemented DynamicMessage so that we can prove
            // it still works in that case.
            int nanos = (int)value.Descriptor.Fields[Timestamp.NanosFieldNumber].Accessor.GetValue(value);
            long seconds = (long)value.Descriptor.Fields[Timestamp.SecondsFieldNumber].Accessor.GetValue(value);
            writer.Write(Timestamp.ToJson(seconds, nanos, DiagnosticOnly));
        }

        private void WriteDuration(Utf8JsonWriter writer, IMessage value)
        {
            // TODO: Same as for WriteTimestamp
            int nanos = (int)value.Descriptor.Fields[Duration.NanosFieldNumber].Accessor.GetValue(value);
            long seconds = (long)value.Descriptor.Fields[Duration.SecondsFieldNumber].Accessor.GetValue(value);
            writer.Write(Duration.ToJson(seconds, nanos, DiagnosticOnly));
        }

        private void WriteFieldMask(Utf8JsonWriter writer, IMessage value)
        {
            var paths = (IList<string>)value.Descriptor.Fields[FieldMask.PathsFieldNumber].Accessor.GetValue(value);
            writer.Write(FieldMask.ToJson(paths, DiagnosticOnly));
        }

        private void WriteAny(Utf8JsonWriter writer, IMessage value)
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

            if (descriptor.IsWellKnownType)
            {
                writer.Write(PropertySeparator);
                WriteString(writer, AnyWellKnownTypeValueField);
                writer.Write(NameValueSeparator);
                WriteWellKnownTypeValue(writer, descriptor, message);
            }
            else
            {
                WriteMessageFields(writer, message, true);
            }
            writer.WriteEndObject();
        }

        private void WriteStruct(Utf8JsonWriter writer, IMessage message)
        {
            IDictionary fields = (IDictionary)message.Descriptor.Fields[Struct.FieldsFieldNumber].Accessor.GetValue(message);

            JsonSerializer.Serialize(writer, fields, _options);

            //writer.WriteStartObject();
            //foreach (DictionaryEntry entry in fields)
            //{
            //    string key = (string)entry.Key;
            //    IMessage value = (IMessage)entry.Value;
            //    if (string.IsNullOrEmpty(key) || value == null)
            //    {
            //        throw new InvalidOperationException("Struct fields cannot have an empty key or a null value.");
            //    }

                
            //    WriteString(writer, key);
            //    writer.Write(NameValueSeparator);
            //    WriteStructFieldValue(writer, value);
            //    first = false;
            //}
            //writer.WriteEndObject();
        }

        private void WriteStructFieldValue(Utf8JsonWriter writer, IMessage message)
        {
            var specifiedField = message.Descriptor.Oneofs[0].Accessor.GetCaseFieldDescriptor(message);
            if (specifiedField == null)
            {
                throw new InvalidOperationException("Value message must contain a value for the oneof.");
            }

            object value = specifiedField.Accessor.GetValue(message);

            switch (specifiedField.FieldNumber)
            {
                case Value.BoolValueFieldNumber:
                case Value.StringValueFieldNumber:
                case Value.NumberValueFieldNumber:
                    WriteValue(writer, value);
                    return;
                case Value.StructValueFieldNumber:
                case Value.ListValueFieldNumber:
                    // Structs and ListValues are nested messages, and already well-known types.
                    var nestedMessage = (IMessage)specifiedField.Accessor.GetValue(message);
                    WriteWellKnownTypeValue(writer, nestedMessage.Descriptor, nestedMessage);
                    return;
                case Value.NullValueFieldNumber:
                    WriteNull(writer);
                    return;
                default:
                    throw new InvalidOperationException("Unexpected case in struct field: " + specifiedField.FieldNumber);
            }
        }
        */

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
                        return GetFieldType(descriptor.MessageType.Fields[ConverterHelpers.WrapperValueFieldNumber]);
                    }
                    return descriptor.MessageType.ClrType;
                default:
                    throw new ArgumentException("Invalid field type");
            }
        }
    }
}
