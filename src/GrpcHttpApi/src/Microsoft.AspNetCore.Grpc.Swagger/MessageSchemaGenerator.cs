// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Microsoft.OpenApi.Models;

namespace Microsoft.AspNetCore.Grpc.HttpApi
{
    /// <summary>
    /// This currently isn't used. Consider deleting once the final method of generating swagger is decided.
    /// </summary>
    internal class MessageSchemaGenerator
    {
        private readonly MessageDescriptor _type;
        private readonly Dictionary<MessageDescriptor, OpenApiSchema> _schemaCache;

        public MessageSchemaGenerator(MessageDescriptor type)
        {
            _type = type;
            _schemaCache = new Dictionary<MessageDescriptor, OpenApiSchema>();
        }

        public OpenApiSchema GenerateSchema()
        {
            return ConvertMessage(_type);
        }

        private OpenApiSchema ConvertMessage(MessageDescriptor messageDescriptor)
        {
            if (IsWellKnownType(messageDescriptor))
            {
                if (IsWrapperType(messageDescriptor))
                {
                    var field = messageDescriptor.Fields[Int32Value.ValueFieldNumber];

                    return ConvertField(field);
                }
                if (messageDescriptor.FullName == Timestamp.Descriptor.FullName ||
                    messageDescriptor.FullName == Duration.Descriptor.FullName ||
                    messageDescriptor.FullName == FieldMask.Descriptor.FullName)
                {
                    return CreateValue("string");
                }
                if (messageDescriptor.FullName == Struct.Descriptor.FullName)
                {
                    return CreateValue("object");
                }
                if (messageDescriptor.FullName == ListValue.Descriptor.FullName)
                {
                    return CreateValue("array");
                }
                if (messageDescriptor.FullName == Value.Descriptor.FullName)
                {
                    return new OpenApiSchema();
                }
                if (messageDescriptor.FullName == Any.Descriptor.FullName)
                {
                    return new OpenApiSchema
                    {
                        Type = "object",
                        Properties =
                        {
                            ["@type"] = CreateValue("string")
                        }
                    };
                }
            }

            if (_schemaCache.TryGetValue(messageDescriptor, out var schema))
            {
                return schema;
            }

            schema = new OpenApiSchema
            {
                Type = "object"
            };
            _schemaCache[messageDescriptor] = schema;

            var properties = new Dictionary<string, OpenApiSchema>();

            foreach (var field in messageDescriptor.Fields.InFieldNumberOrder())
            {
                var fieldSchema = ConvertField(field);

                properties[field.JsonName] = fieldSchema;
            }

            schema.Properties = properties;
            schema.AdditionalPropertiesAllowed = false;
            return schema;
        }

        private static readonly HashSet<string> WellKnownTypeNames = new HashSet<string>
        {
            "google/protobuf/any.proto",
            "google/protobuf/api.proto",
            "google/protobuf/duration.proto",
            "google/protobuf/empty.proto",
            "google/protobuf/wrappers.proto",
            "google/protobuf/timestamp.proto",
            "google/protobuf/field_mask.proto",
            "google/protobuf/source_context.proto",
            "google/protobuf/struct.proto",
            "google/protobuf/type.proto",
        };

        internal bool IsWellKnownType(MessageDescriptor messageDescriptor) => messageDescriptor.File.Package == "google.protobuf" &&
            WellKnownTypeNames.Contains(messageDescriptor.File.Name);

        internal bool IsWrapperType(MessageDescriptor messageDescriptor) => messageDescriptor.File.Package == "google.protobuf" &&
            messageDescriptor.File.Name == "google/protobuf/wrappers.proto";

        private OpenApiSchema ConvertField(FieldDescriptor field)
        {
            OpenApiSchema fieldSchema;

            switch (field.FieldType)
            {
                case FieldType.Double:
                case FieldType.Float:
                case FieldType.Int64:
                case FieldType.UInt64:
                case FieldType.Int32:
                case FieldType.Fixed64:
                case FieldType.Fixed32:
                case FieldType.UInt32:
                case FieldType.SFixed32:
                case FieldType.SFixed64:
                case FieldType.SInt32:
                case FieldType.SInt64:
                    fieldSchema = CreateValue("number");
                    break;
                case FieldType.Bool:
                    fieldSchema = CreateValue("boolean");
                    break;
                case FieldType.String:
                case FieldType.Bytes:
                case FieldType.Enum:
                    fieldSchema = CreateValue("string");
                    break;
                case FieldType.Message:
                    fieldSchema = ConvertMessage(field.MessageType);
                    break;
                default:
                    throw new InvalidOperationException("Unexpected field type: " + field.FieldType);
            }

            if (field.IsRepeated)
            {
                fieldSchema = new OpenApiSchema
                {
                    Type = "array",
                    Items = fieldSchema
                };
            }

            return fieldSchema;
        }

        private OpenApiSchema CreateValue(string type)
        {
            return new OpenApiSchema
            {
                Type = type
            };
        }
    }
}
