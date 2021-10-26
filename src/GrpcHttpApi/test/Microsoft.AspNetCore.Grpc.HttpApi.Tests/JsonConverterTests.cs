// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using HttpApi;
using Microsoft.AspNetCore.Grpc.HttpApi;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

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

        public override bool CanConvert(Type typeToConvert)
        {
            var result = base.CanConvert(typeToConvert);
            
            return typeof(IMessage).IsAssignableFrom(typeToConvert);
        }

        private void WriteValue(Utf8JsonWriter writer, object value)
        {
            writer.WriteStringValue(value.ToString());
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
                Name = "test"
            };

            var json = JsonSerializer.Serialize(helloRequest, jsonSerializerOptions);
            _output.WriteLine(json);
        }
    }
}
