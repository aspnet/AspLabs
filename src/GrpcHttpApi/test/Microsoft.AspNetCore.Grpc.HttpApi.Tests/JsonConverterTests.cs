// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using HttpApi;
using Microsoft.AspNetCore.Grpc.HttpApi;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;
using Type = System.Type;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Tests
{
    public class JsonSettings
    {
        /// <summary>
        /// Whether fields which would otherwise not be included in the formatted data
        /// should be formatted even when the value is not present, or has the default value.
        /// This option only affects fields which don't support "presence" (e.g.
        /// singular non-optional proto3 primitive fields).
        /// </summary>
        public bool FormatDefaultValues { get; }
    }

    public class MessageConverter : JsonConverter<IMessage>
    {
        private readonly JsonSettings _settings;

        public MessageConverter(JsonSettings settings)
        {
            _settings = settings;
        }

        public override IMessage Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            return new HelloRequest();
        }

        public override void Write(
            Utf8JsonWriter writer,
            IMessage value,
            JsonSerializerOptions options)
        {
            WriteMessage(writer, value);
        }

        private void WriteMessage(Utf8JsonWriter writer, IMessage message)
        {
            writer.WriteStartObject();

            var fields = message.Descriptor.Fields;
            
            foreach (var field in fields.InFieldNumberOrder())
            {
                var accessor = field.Accessor;
                var value = accessor.GetValue(message);
                if (!ShouldFormatFieldValue(message, field, value))
                {
                    continue;
                }

                writer.WritePropertyName(accessor.Descriptor.JsonName);
                WriteValue(writer, value);
            }

            writer.WriteEndObject();
        }

        public override bool CanConvert(System.Type typeToConvert)
        {
            return typeof(IMessage).IsAssignableFrom(typeToConvert);
        }

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
            else if (value is IDictionary d)
            {
                WriteDictionary(writer, (IDictionary)value);
            }
            else if (value is IList l)
            {
                WriteList(writer, (IList)value);
            }
            //else if (value is int || value is uint)
            //{
            //    IFormattable formattable = (IFormattable)value;
            //    writer.Write(formattable.ToString("d", CultureInfo.InvariantCulture));
            //}
            //else if (value is long || value is ulong)
            //{
            //    writer.Write('"');
            //    IFormattable formattable = (IFormattable)value;
            //    writer.Write(formattable.ToString("d", CultureInfo.InvariantCulture));
            //    writer.Write('"');
            //}
            //else if (value is System.Enum)
            //{
            //    if (settings.FormatEnumsAsIntegers)
            //    {
            //        WriteValue(writer, (int)value);
            //    }
            //    else
            //    {
            //        string name = OriginalEnumValueHelper.GetOriginalName(value);
            //        if (name != null)
            //        {
            //            WriteString(writer, name);
            //        }
            //        else
            //        {
            //            WriteValue(writer, (int)value);
            //        }
            //    }
            //}
            //else if (value is float || value is double)
            //{
            //    string text = ((IFormattable)value).ToString("r", CultureInfo.InvariantCulture);
            //    if (text == "NaN" || text == "Infinity" || text == "-Infinity")
            //    {
            //        writer.Write('"');
            //        writer.Write(text);
            //        writer.Write('"');
            //    }
            //    else
            //    {
            //        writer.Write(text);
            //    }
            //}
            else if (value is IMessage m)
            {
                WriteMessage(writer, m);
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

        /// <summary>
        /// Determines whether or not a field value should be serialized according to the field,
        /// its value in the message, and the settings of this formatter.
        /// </summary>
        private bool ShouldFormatFieldValue(IMessage message, FieldDescriptor field, object value) =>
            field.HasPresence
            // Fields that support presence *just* use that
            ? field.Accessor.HasValue(message)
            // Otherwise, format if either we've been asked to format default values, or if it's
            // not a default value anyway.
            : _settings.FormatDefaultValues || !IsDefaultValue(field, value);

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

    public class JsonConverterTests
    {
        private readonly ITestOutputHelper _output;

        public JsonConverterTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CancellationToken_Get_MatchHttpContextRequestAborted()
        {
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                Converters =
                {
                    new MessageConverter(new JsonSettings())
                },
                WriteIndented = true
            };

            var helloRequest = new HelloRequest
            {
                Name = "test",
                RepeatedStrings =
                {
                    "One",
                    "Two",
                    "Three"
                }
            };

            var json = JsonSerializer.Serialize(helloRequest, jsonSerializerOptions);
            _output.WriteLine(json);
        }
    }
}
